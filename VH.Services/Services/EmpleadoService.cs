using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmpleadoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Empleado>> GetAllEmpleadosAsync()
        {
            return await _unitOfWork.Empleados.GetAllAsync(includeProperties: "Proyecto");
        }

        public async Task<Empleado?> GetEmpleadoByIdAsync(int id)
        {
            return await _unitOfWork.Empleados.GetByIdAsync(id, includeProperties: "Proyecto");
        }

        public async Task<Empleado> CreateEmpleadoAsync(Empleado empleado)
        {
            // Validar que el proyecto exista
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(empleado.IdProyecto);
            if (proyecto == null)
                throw new ArgumentException($"El proyecto con ID {empleado.IdProyecto} no existe.");

            // Validar número de nómina duplicado
            var existente = await _unitOfWork.Empleados.FindAsync(
                e => e.NumeroNomina.ToLower() == empleado.NumeroNomina.ToLower());

            if (existente.Any())
                throw new InvalidOperationException($"Ya existe un empleado con el número de nómina '{empleado.NumeroNomina}'.");

            await _unitOfWork.Empleados.AddAsync(empleado);
            await _unitOfWork.CompleteAsync();
            return empleado;
        }

        public async Task<bool> UpdateEmpleadoAsync(Empleado empleado)
        {
            var empleadoExistente = await _unitOfWork.Empleados.GetByIdAsync(empleado.IdEmpleado);
            if (empleadoExistente == null)
                return false;

            // Validar que el proyecto exista (si cambió)
            if (empleado.IdProyecto != empleadoExistente.IdProyecto)
            {
                var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(empleado.IdProyecto);
                if (proyecto == null)
                    throw new ArgumentException($"El proyecto con ID {empleado.IdProyecto} no existe.");
            }

            // Validar número de nómina duplicado (excluyendo el actual)
            var duplicado = await _unitOfWork.Empleados.FindAsync(
                e => e.IdEmpleado != empleado.IdEmpleado &&
                     e.NumeroNomina.ToLower() == empleado.NumeroNomina.ToLower());

            if (duplicado.Any())
                throw new InvalidOperationException($"Ya existe otro empleado con el número de nómina '{empleado.NumeroNomina}'.");

            empleadoExistente.Nombre = empleado.Nombre;
            empleadoExistente.ApellidoPaterno = empleado.ApellidoPaterno;
            empleadoExistente.ApellidoMaterno = empleado.ApellidoMaterno;
            empleadoExistente.NumeroNomina = empleado.NumeroNomina;
            empleadoExistente.Puesto = empleado.Puesto;
            empleadoExistente.IdProyecto = empleado.IdProyecto;

            _unitOfWork.Empleados.Update(empleadoExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteEmpleadoAsync(int id)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(id);
            if (empleado == null)
                return false;

            // Validar que no tenga entregas EPP
            var entregas = await _unitOfWork.EntregasEPP.FindAsync(e => e.IdEmpleado == id);
            if (entregas.Any())
                throw new InvalidOperationException("No se puede eliminar el empleado porque tiene entregas EPP registradas.");

            _unitOfWork.Empleados.Remove(empleado);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}