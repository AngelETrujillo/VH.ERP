using VH.Services.Interfaces;
using VH.Services.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VH.Services.Services
{
    public class ConceptoPartidaService : IConceptoPartidaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConceptoPartidaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ConceptoPartida>> GetPartidasByProyectoAsync(int idProyecto)
        {
            // Incluir UnidadMedida para poder mostrar su información
            return await _unitOfWork.ConceptosPartidas.FindAsync(
                filter: cp => cp.IdProyecto == idProyecto,
                includeProperties: "UnidadMedida"
            );
        }

        public async Task<ConceptoPartida?> GetPartidaByIdAsync(int idPartida)
        {
            // Incluir UnidadMedida para mapeo completo
            return await _unitOfWork.ConceptosPartidas.GetByIdAsync(
                idPartida,
                includeProperties: "UnidadMedida"
            );
        }

        public async Task<ConceptoPartida?> CreatePartidaAsync(int idProyecto, ConceptoPartida nuevaPartida)
        {
            // 1. Verificar si el proyecto existe
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(idProyecto);
            if (proyecto == null)
            {
                return null; // El proyecto padre no existe
            }

            // 2. Verificar si la UnidadMedida existe
            var unidadMedida = await _unitOfWork.UnidadesMedida.GetByIdAsync(nuevaPartida.IdUnidadMedida);
            if (unidadMedida == null)
            {
                return null; // La unidad de medida no existe
            }

            // 3. Asignar la FK y agregar la entidad
            nuevaPartida.IdProyecto = idProyecto;
            await _unitOfWork.ConceptosPartidas.AddAsync(nuevaPartida);
            await _unitOfWork.CompleteAsync(); // Guardar cambios en la DB

            // 4. Recalcular presupuesto total del proyecto
            await RecalcularPresupuestoTotalAsync(idProyecto);

            return nuevaPartida;
        }

        public async Task<bool> UpdatePartidaAsync(ConceptoPartida partidaActualizada)
        {
            var partidaExistente = await _unitOfWork.ConceptosPartidas.GetByIdAsync(partidaActualizada.IdPartida);

            if (partidaExistente == null)
            {
                return false;
            }

            // Verificar si la UnidadMedida existe (si se está cambiando)
            if (partidaActualizada.IdUnidadMedida != partidaExistente.IdUnidadMedida)
            {
                var unidadMedida = await _unitOfWork.UnidadesMedida.GetByIdAsync(partidaActualizada.IdUnidadMedida);
                if (unidadMedida == null)
                {
                    return false; // La unidad de medida no existe
                }
            }

            // Mapear los campos permitidos para la actualización
            partidaExistente.Descripcion = partidaActualizada.Descripcion;
            partidaExistente.IdUnidadMedida = partidaActualizada.IdUnidadMedida; // ✅ CORREGIDO: Era string, ahora es int
            partidaExistente.CantidadEstimada = partidaActualizada.CantidadEstimada;
            // NOTA: No se debe permitir cambiar IdProyecto aquí

            _unitOfWork.ConceptosPartidas.Update(partidaExistente);
            await _unitOfWork.CompleteAsync();

            await RecalcularPresupuestoTotalAsync(partidaExistente.IdProyecto);

            return true;
        }

        public async Task<bool> DeletePartidaAsync(int idPartida)
        {
            var partida = await _unitOfWork.ConceptosPartidas.GetByIdAsync(idPartida);
            if (partida == null)
            {
                return false;
            }

            int idProyecto = partida.IdProyecto;
            _unitOfWork.ConceptosPartidas.Remove(partida);
            await _unitOfWork.CompleteAsync();

            await RecalcularPresupuestoTotalAsync(idProyecto);

            return true;
        }

        private async Task RecalcularPresupuestoTotalAsync(int idProyecto)
        {
            // 1. Obtener todas las partidas del proyecto
            var partidas = await GetPartidasByProyectoAsync(idProyecto);

            // 2. Calcular la suma total
            decimal nuevoTotal = partidas.Sum(cp => cp.CantidadEstimada);

            // 3. Obtener el proyecto padre
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(idProyecto);

            if (proyecto != null)
            {
                // 4. Actualizar el campo y persistir los cambios
                proyecto.PresupuestoTotal = nuevoTotal;
                _unitOfWork.Proyectos.Update(proyecto);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}