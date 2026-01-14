using VH.Services.Entities;
using VH.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VH.Services.Services
{
    public class UnidadMedidaService : IUnidadMedidaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnidadMedidaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UnidadMedida>> GetAllUnidadesMedidaAsync()
        {
            return await _unitOfWork.UnidadesMedida.GetAllAsync();
        }

        public async Task<UnidadMedida?> GetUnidadMedidaByIdAsync(int id)
        {
            return await _unitOfWork.UnidadesMedida.GetByIdAsync(id);
        }

        public async Task<UnidadMedida> CreateUnidadMedidaAsync(UnidadMedida unidadMedida)
        {
            // Validar que no exista una unidad con el mismo nombre o abreviatura
            var existente = await _unitOfWork.UnidadesMedida.FindAsync(
                um => um.Nombre.ToLower() == unidadMedida.Nombre.ToLower() ||
                      um.Abreviatura.ToLower() == unidadMedida.Abreviatura.ToLower()
            );

            if (existente.Any())
            {
                throw new InvalidOperationException("Ya existe una unidad de medida con ese nombre o abreviatura.");
            }

            await _unitOfWork.UnidadesMedida.AddAsync(unidadMedida);
            await _unitOfWork.CompleteAsync();
            return unidadMedida;
        }

        public async Task<bool> UpdateUnidadMedidaAsync(UnidadMedida unidadMedida)
        {
            var unidadExistente = await _unitOfWork.UnidadesMedida.GetByIdAsync(unidadMedida.IdUnidadMedida);
            if (unidadExistente == null)
            {
                return false;
            }

            // Validar que no exista otra unidad con el mismo nombre o abreviatura
            var duplicado = await _unitOfWork.UnidadesMedida.FindAsync(
                um => um.IdUnidadMedida != unidadMedida.IdUnidadMedida &&
                      (um.Nombre.ToLower() == unidadMedida.Nombre.ToLower() ||
                       um.Abreviatura.ToLower() == unidadMedida.Abreviatura.ToLower())
            );

            if (duplicado.Any())
            {
                return false; // Ya existe otra unidad con ese nombre/abreviatura
            }

            // Actualizar campos
            unidadExistente.Nombre = unidadMedida.Nombre;
            unidadExistente.Abreviatura = unidadMedida.Abreviatura;
            unidadExistente.Descripcion = unidadMedida.Descripcion;

            _unitOfWork.UnidadesMedida.Update(unidadExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteUnidadMedidaAsync(int id)
        {
            var unidadMedida = await _unitOfWork.UnidadesMedida.GetByIdAsync(id);
            if (unidadMedida == null)
            {
                return false;
            }

            // Verificar si está siendo usada por materiales o conceptos
            var materialesUsandola = await _unitOfWork.MaterialesEPP.FindAsync(m => m.IdUnidadMedida == id);
            var conceptosUsandola = await _unitOfWork.ConceptosPartidas.FindAsync(c => c.IdUnidadMedida == id);

            if (materialesUsandola.Any() || conceptosUsandola.Any())
            {
                throw new InvalidOperationException("No se puede eliminar la unidad de medida porque está siendo utilizada.");
            }

            _unitOfWork.UnidadesMedida.Remove(unidadMedida);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}