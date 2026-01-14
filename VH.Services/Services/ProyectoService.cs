using VH.Services.Interfaces;
using VH.Services.Entities;

namespace VH.Services.Services
{
    public class ProyectoService : IProyectoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProyectoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Proyecto>> GetAllProyectosAsync()
        {
            return await _unitOfWork.Proyectos.GetAllAsync();
        }

        public async Task<Proyecto?> GetProyectoByIdAsync(int id)
        {
            return await _unitOfWork.Proyectos.GetByIdAsync(id);
        }

        public async Task<Proyecto> CreateProyectoAsync(Proyecto nuevoProyecto)
        {
            await _unitOfWork.Proyectos.AddAsync(nuevoProyecto);
            await _unitOfWork.CompleteAsync(); // Guardar cambios en la DB
            return nuevoProyecto;
        }

        public async Task<bool> UpdateProyectoAsync(Proyecto proyectoActualizado)
        {
            var proyectoExistente = await _unitOfWork.Proyectos.GetByIdAsync(proyectoActualizado.IdProyecto);

            if (proyectoExistente == null)
            {
                return false;
            }

            // Mapear manualmente o usar un mapper (como Automapper)
            proyectoExistente.Nombre = proyectoActualizado.Nombre;
            proyectoExistente.TipoObra = proyectoActualizado.TipoObra;
            proyectoExistente.FechaInicio = proyectoActualizado.FechaInicio;
            proyectoExistente.PresupuestoTotal = proyectoActualizado.PresupuestoTotal;

            _unitOfWork.Proyectos.Update(proyectoExistente);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteProyectoAsync(int id)
        {
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(id);
            if (proyecto == null)
            {
                return false;
            }

            _unitOfWork.Proyectos.Remove(proyecto);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}