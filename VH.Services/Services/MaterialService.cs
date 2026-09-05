using VH.Services.Entities;
using VH.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VH.Services.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Material>> GetAllMaterialesAsync(TipoMaterial? tipo = null)
        {
            // Incluir UnidadMedida e Inventarios para cálculo de stock
            if (tipo.HasValue)
            {
                return await _unitOfWork.Materiales.FindAsync(
                    m => m.TipoMaterial == tipo.Value,
                    includeProperties: "UnidadMedida,Inventarios"
                );
            }

            return await _unitOfWork.Materiales.GetAllAsync(
                includeProperties: "UnidadMedida,Inventarios"
            );
        }

        public async Task<Material?> GetMaterialByIdAsync(int id)
        {
            // Incluir UnidadMedida e Inventarios para cálculo de stock
            return await _unitOfWork.Materiales.GetByIdAsync(
                id,
                includeProperties: "UnidadMedida,Inventarios"
            );
        }

        public async Task<Material> CreateMaterialAsync(Material material)
        {
            // Verificar si la UnidadMedida existe
            var unidadMedida = await _unitOfWork.UnidadesMedida.GetByIdAsync(material.IdUnidadMedida);
            if (unidadMedida == null)
                throw new ArgumentException($"La unidad de medida con ID {material.IdUnidadMedida} no existe.");

            // Validar nombre duplicado
            var existente = await _unitOfWork.Materiales.FindAsync(
                m => m.Nombre.ToLower() == material.Nombre.ToLower());

            if (existente.Any())
                throw new InvalidOperationException($"Ya existe un material con el nombre '{material.Nombre}'.");

            await _unitOfWork.Materiales.AddAsync(material);
            await _unitOfWork.CompleteAsync();
            return material;
        }

        public async Task<bool> UpdateMaterialAsync(Material material)
        {
            var materialExistente = await _unitOfWork.Materiales.GetByIdAsync(material.IdMaterial);
            if (materialExistente == null)
            {
                return false;
            }

            // Verificar si la UnidadMedida existe (si se está cambiando)
            if (material.IdUnidadMedida != materialExistente.IdUnidadMedida)
            {
                var unidadMedida = await _unitOfWork.UnidadesMedida.GetByIdAsync(material.IdUnidadMedida);
                if (unidadMedida == null)
                {
                    return false;
                }
            }

            // Actualizar campos
            materialExistente.Nombre = material.Nombre;
            materialExistente.Descripcion = material.Descripcion;
            materialExistente.IdUnidadMedida = material.IdUnidadMedida;
            materialExistente.CostoUnitarioEstimado = material.CostoUnitarioEstimado;
            materialExistente.Activo = material.Activo;

            _unitOfWork.Materiales.Update(materialExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            var material = await _unitOfWork.Materiales.GetByIdAsync(id);
            if (material == null)
            {
                return false;
            }

            // Verificar si tiene registros de inventario
            var inventarios = await _unitOfWork.Inventarios.FindAsync(i => i.IdMaterial == id);
            if (inventarios.Any())
            {
                throw new InvalidOperationException("No se puede eliminar el material porque tiene registros de inventario asociados.");
            }

            // Verificar si tiene entregas EPP (a través de las compras)
            var compras = await _unitOfWork.ComprasEPP.FindAsync(c => c.IdMaterial == id);
            if (compras.Any())
            {
                throw new InvalidOperationException("No se puede eliminar el material porque tiene compras/entregas EPP asociadas.");
            }

            _unitOfWork.Materiales.Remove(material);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}