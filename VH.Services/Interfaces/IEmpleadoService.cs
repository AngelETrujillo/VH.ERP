using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IEmpleadoService
    {
        Task<IEnumerable<Empleado>> GetAllEmpleadosAsync();
        Task<Empleado?> GetEmpleadoByIdAsync(int id);
        Task<Empleado> CreateEmpleadoAsync(Empleado empleado);
        Task<bool> UpdateEmpleadoAsync(Empleado empleado);
        Task<bool> DeleteEmpleadoAsync(int id);
    }
}
