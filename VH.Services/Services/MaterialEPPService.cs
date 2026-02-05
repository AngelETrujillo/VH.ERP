using VH.Services.Entities;
using VH.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VH.Services.Services
{
    public class MaterialEPPService : IMaterialEPPService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialEPPService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MaterialEPP>> GetAllMaterialesEPPAsync()
        {
            // Incluir UnidadMedida e Inventarios para cálculo de stock
            return await _unitOfWork.MaterialesEPP.GetAllAsync(
                includeProperties: "UnidadMedida,Inventarios"
            );
        }

        public async Task<MaterialEPP?> GetMaterialEPPByIdAsync(int id)
        {
            // Incluir UnidadMedida e Inventarios para cálculo de stock
            return await _unitOfWork.MaterialesEPP.GetByIdAsync(
                id,
                includeProperties: "UnidadMedida,Inventarios"
            );
        }

        public async Task<MaterialEPP> CreateMaterialEPPAsync(MaterialEPP material)
        {
            // Verificar si la UnidadMedida existe
            var unidadMedida = await _unitOfWork.UnidadesMedida.GetByIdAsync(material.IdUnidadMedida);
            if (unidadMedida == null)
                throw new ArgumentException($"La unidad de medida con ID {material.IdUnidadMedida} no existe.");

            // Validar nombre duplicado
            var existente = await _unitOfWork.MaterialesEPP.FindAsync(
                m => m.Nombre.ToLower() == material.Nombre.ToLower());

            if (existente.Any())
                throw new InvalidOperationException($"Ya existe un material con el nombre '{material.Nombre}'.");

            await _unitOfWork.MaterialesEPP.AddAsync(material);
            await _unitOfWork.CompleteAsync();
            return material;
        }

        public async Task<bool> UpdateMaterialEPPAsync(MaterialEPP material)
        {
            var materialExistente = await _unitOfWork.MaterialesEPP.GetByIdAsync(material.IdMaterial);
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

            _unitOfWork.MaterialesEPP.Update(materialExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteMaterialEPPAsync(int id)
        {
            var material = await _unitOfWork.MaterialesEPP.GetByIdAsync(id);
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

            _unitOfWork.MaterialesEPP.Remove(material);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}