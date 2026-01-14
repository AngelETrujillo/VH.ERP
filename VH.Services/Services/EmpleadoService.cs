using VH.Services.Entities;
using VH.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            // Incluye la relación con Proyecto para la carga perezosa/explícita si es necesario.
            return await _unitOfWork.Empleados.GetAllAsync(includeProperties: "Proyecto");
        }

        public async Task<Empleado?> GetEmpleadoByIdAsync(int id)
        {
            return await _unitOfWork.Empleados.GetByIdAsync(id, includeProperties: "Proyecto");
        }

        public async Task<Empleado> CreateEmpleadoAsync(Empleado empleado)
        {
            await _unitOfWork.Empleados.AddAsync(empleado);
            await _unitOfWork.CompleteAsync();
            return empleado;
        }

        public async Task<bool> UpdateEmpleadoAsync(Empleado empleado)
        {
            _unitOfWork.Empleados.Update(empleado);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteEmpleadoAsync(int id)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(id);
            if (empleado == null) return false;

            _unitOfWork.Empleados.Remove(empleado);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}