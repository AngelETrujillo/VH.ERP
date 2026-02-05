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
            // Validar nombre duplicado
            var existente = await _unitOfWork.Proyectos.FindAsync(
                p => p.Nombre.ToLower() == nuevoProyecto.Nombre.ToLower());

            if (existente.Any())
                throw new InvalidOperationException($"Ya existe un proyecto con el nombre '{nuevoProyecto.Nombre}'.");

            await _unitOfWork.Proyectos.AddAsync(nuevoProyecto);
            await _unitOfWork.CompleteAsync();
            return nuevoProyecto;
        }

        public async Task<bool> UpdateProyectoAsync(Proyecto proyectoActualizado)
        {
            var proyectoExistente = await _unitOfWork.Proyectos.GetByIdAsync(proyectoActualizado.IdProyecto);
            if (proyectoExistente == null)
                return false;

            // Validar nombre duplicado (excluyendo el actual)
            var duplicado = await _unitOfWork.Proyectos.FindAsync(
                p => p.IdProyecto != proyectoActualizado.IdProyecto &&
                     p.Nombre.ToLower() == proyectoActualizado.Nombre.ToLower());

            if (duplicado.Any())
                throw new InvalidOperationException($"Ya existe otro proyecto con el nombre '{proyectoActualizado.Nombre}'.");

            proyectoExistente.Nombre = proyectoActualizado.Nombre;
            proyectoExistente.TipoObra = proyectoActualizado.TipoObra;
            proyectoExistente.FechaInicio = proyectoActualizado.FechaInicio;
            proyectoExistente.PresupuestoTotal = proyectoActualizado.PresupuestoTotal;

            _unitOfWork.Proyectos.Update(proyectoExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteProyectoAsync(int id)
        {
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(id);
            if (proyecto == null)
                return false;

            // Validar que no tenga empleados
            var empleados = await _unitOfWork.Empleados.FindAsync(e => e.IdProyecto == id);
            if (empleados.Any())
                throw new InvalidOperationException("No se puede eliminar el proyecto porque tiene empleados asignados.");

            // Validar que no tenga almacenes
            var almacenes = await _unitOfWork.Almacenes.FindAsync(a => a.IdProyecto == id);
            if (almacenes.Any())
                throw new InvalidOperationException("No se puede eliminar el proyecto porque tiene almacenes asociados.");

            // Validar que no tenga partidas
            var partidas = await _unitOfWork.ConceptosPartidas.FindAsync(cp => cp.IdProyecto == id);
            if (partidas.Any())
                throw new InvalidOperationException("No se puede eliminar el proyecto porque tiene partidas asociadas.");

            _unitOfWork.Proyectos.Remove(proyecto);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}