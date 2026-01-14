using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IEntregaEPPService
    {
        // Obtiene todas las entregas o filtra por empleado
        Task<IEnumerable<EntregaEPP>> GetEntregasAsync(int? idEmpleado = null);
        Task<EntregaEPP> GetEntregaEPPByIdAsync(int id);
        Task<EntregaEPP> CreateEntregaEPPAsync(EntregaEPP entrega);
        Task<bool> UpdateEntregaEPPAsync(EntregaEPP entrega);
        Task<bool> DeleteEntregaEPPAsync(int id);
    }
}
