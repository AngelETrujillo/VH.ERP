using VH.Services.Entities;
using VH.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;

namespace VH.Services.Services
{
    public class EntregaEPPService : IEntregaEPPService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EntregaEPPService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Propiedades de navegación para el mapeo correcto
        private const string IncludeProperties = "Empleado.Proyecto,MaterialEPP.UnidadMedida,Proveedor";

        public async Task<IEnumerable<EntregaEPP>> GetEntregasAsync(int? idEmpleado = null)
        {
            Expression<Func<EntregaEPP, bool>>? filter = null;
            if (idEmpleado.HasValue)
            {
                filter = e => e.IdEmpleado == idEmpleado.Value;
            }

            return await _unitOfWork.EntregasEPP.FindAsync(
                filter: filter,
                includeProperties: IncludeProperties);
        }

        public async Task<EntregaEPP?> GetEntregaEPPByIdAsync(int id)
        {
            return await _unitOfWork.EntregasEPP.GetByIdAsync(id, includeProperties: IncludeProperties);
        }

        public async Task<EntregaEPP> CreateEntregaEPPAsync(EntregaEPP entrega)
        {
            // Validar existencia de FKs
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(entrega.IdEmpleado);
            if (empleado == null)
            {
                throw new ArgumentException($"El empleado con ID {entrega.IdEmpleado} no existe.");
            }

            var material = await _unitOfWork.MaterialesEPP.GetByIdAsync(entrega.IdMaterial);
            if (material == null)
            {
                throw new ArgumentException($"El material con ID {entrega.IdMaterial} no existe.");
            }

            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(entrega.IdProveedor);
            if (proveedor == null)
            {
                throw new ArgumentException($"El proveedor con ID {entrega.IdProveedor} no existe.");
            }

            await _unitOfWork.EntregasEPP.AddAsync(entrega);
            await _unitOfWork.CompleteAsync();
            return entrega;
        }

        public async Task<bool> UpdateEntregaEPPAsync(EntregaEPP entrega)
        {
            var entregaExistente = await _unitOfWork.EntregasEPP.GetByIdAsync(entrega.IdEntrega);
            if (entregaExistente == null)
            {
                return false;
            }

            // Actualizar campos
            entregaExistente.IdEmpleado = entrega.IdEmpleado;
            entregaExistente.IdMaterial = entrega.IdMaterial;
            entregaExistente.IdProveedor = entrega.IdProveedor;
            entregaExistente.FechaEntrega = entrega.FechaEntrega;
            entregaExistente.CantidadEntregada = entrega.CantidadEntregada;
            entregaExistente.TallaEntregada = entrega.TallaEntregada;
            entregaExistente.Observaciones = entrega.Observaciones;

            _unitOfWork.EntregasEPP.Update(entregaExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteEntregaEPPAsync(int id)
        {
            var entrega = await _unitOfWork.EntregasEPP.GetByIdAsync(id);
            if (entrega == null) return false;

            _unitOfWork.EntregasEPP.Remove(entrega);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}