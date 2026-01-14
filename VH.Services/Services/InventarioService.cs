using VH.Services.Entities;
using VH.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VH.Services.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventarioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Inventario>> GetAllInventariosAsync()
        {
            return await _unitOfWork.Inventarios.GetAllAsync(
                includeProperties: "Almacen.Proyecto,Material.UnidadMedida"
            );
        }

        public async Task<IEnumerable<Inventario>> GetInventariosByAlmacenAsync(int idAlmacen)
        {
            return await _unitOfWork.Inventarios.FindAsync(
                filter: i => i.IdAlmacen == idAlmacen,
                includeProperties: "Material.UnidadMedida"
            );
        }

        public async Task<IEnumerable<Inventario>> GetInventariosByMaterialAsync(int idMaterial)
        {
            return await _unitOfWork.Inventarios.FindAsync(
                filter: i => i.IdMaterial == idMaterial,
                includeProperties: "Almacen.Proyecto"
            );
        }

        public async Task<Inventario?> GetInventarioByIdAsync(int id)
        {
            return await _unitOfWork.Inventarios.GetByIdAsync(
                id,
                includeProperties: "Almacen.Proyecto,Material.UnidadMedida"
            );
        }

        public async Task<Inventario> CreateInventarioAsync(Inventario inventario)
        {
            // Validar que el almacén exista
            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(inventario.IdAlmacen);
            if (almacen == null)
            {
                throw new ArgumentException($"El almacén con ID {inventario.IdAlmacen} no existe.");
            }

            // Validar que el material exista
            var material = await _unitOfWork.MaterialesEPP.GetByIdAsync(inventario.IdMaterial);
            if (material == null)
            {
                throw new ArgumentException($"El material con ID {inventario.IdMaterial} no existe.");
            }

            // Validar que no exista ya un registro para ese material en ese almacén
            var inventarioExistente = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdAlmacen == inventario.IdAlmacen && i.IdMaterial == inventario.IdMaterial
            );

            if (inventarioExistente.Any())
            {
                throw new InvalidOperationException("Ya existe un registro de inventario para este material en este almacén.");
            }

            // Validaciones de negocio
            if (inventario.StockMinimo < 0)
            {
                throw new ArgumentException("El stock mínimo no puede ser negativo.");
            }

            if (inventario.StockMaximo < inventario.StockMinimo)
            {
                throw new ArgumentException("El stock máximo no puede ser menor que el stock mínimo.");
            }

            // Establecer fecha de último movimiento
            inventario.FechaUltimoMovimiento = DateTime.Now;

            await _unitOfWork.Inventarios.AddAsync(inventario);
            await _unitOfWork.CompleteAsync();
            return inventario;
        }

        public async Task<bool> UpdateInventarioAsync(Inventario inventario)
        {
            var inventarioExistente = await _unitOfWork.Inventarios.GetByIdAsync(inventario.IdInventario);
            if (inventarioExistente == null)
            {
                return false;
            }

            // Validaciones de negocio
            if (inventario.StockMinimo < 0)
            {
                throw new ArgumentException("El stock mínimo no puede ser negativo.");
            }

            if (inventario.StockMaximo < inventario.StockMinimo)
            {
                throw new ArgumentException("El stock máximo no puede ser menor que el stock mínimo.");
            }

            if (inventario.Existencia < 0)
            {
                throw new ArgumentException("La existencia no puede ser negativa.");
            }

            // Actualizar campos
            inventarioExistente.Existencia = inventario.Existencia;
            inventarioExistente.StockMinimo = inventario.StockMinimo;
            inventarioExistente.StockMaximo = inventario.StockMaximo;
            inventarioExistente.UbicacionPasillo = inventario.UbicacionPasillo;
            inventarioExistente.FechaUltimoMovimiento = DateTime.Now;

            _unitOfWork.Inventarios.Update(inventarioExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteInventarioAsync(int id)
        {
            var inventario = await _unitOfWork.Inventarios.GetByIdAsync(id);
            if (inventario == null)
            {
                return false;
            }

            _unitOfWork.Inventarios.Remove(inventario);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<decimal> GetStockGlobalMaterialAsync(int idMaterial)
        {
            // Obtener todos los inventarios de un material en todos los almacenes
            var inventarios = await _unitOfWork.Inventarios.FindAsync(i => i.IdMaterial == idMaterial);

            // Sumar las existencias
            return inventarios.Sum(i => i.Existencia);
        }
    }
}