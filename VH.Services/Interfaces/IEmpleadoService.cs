using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IEmpleadoService
    {
        /// <summary>Una página del catálogo, filtrada y ordenada en la base.</summary>
        Task<ResultadoPaginado<Empleado>> GetPaginadoAsync(ConsultaPaginada consulta, bool? activo = null);

        Task<IEnumerable<Empleado>> GetAllEmpleadosAsync();
        Task<Empleado?> GetEmpleadoByIdAsync(int id);
        Task<Empleado> CreateEmpleadoAsync(Empleado empleado);
        Task<bool> UpdateEmpleadoAsync(Empleado empleado);
        Task<bool> DeleteEmpleadoAsync(int id);
    }
}
