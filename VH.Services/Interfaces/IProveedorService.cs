using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IProveedorService
    {
        Task<IEnumerable<Proveedor>> GetAllProveedoresAsync();
        Task<Proveedor?> GetProveedorByIdAsync(int id);
        Task<Proveedor> CreateProveedorAsync(Proveedor proveedor);
        Task<bool> UpdateProveedorAsync(Proveedor proveedor);
        Task<bool> DeleteProveedorAsync(int id);
    }
}
