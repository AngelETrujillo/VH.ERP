using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IProveedorService
    {
        /// <summary>Una página del catálogo, filtrada y ordenada en la base.</summary>
        Task<ResultadoPaginado<Proveedor>> GetPaginadoAsync(ConsultaPaginada consulta, bool? activo = null);

        Task<IEnumerable<Proveedor>> GetAllProveedoresAsync();
        Task<Proveedor?> GetProveedorByIdAsync(int id);
        Task<Proveedor> CreateProveedorAsync(Proveedor proveedor);
        Task<bool> UpdateProveedorAsync(Proveedor proveedor);
        Task<bool> DeleteProveedorAsync(int id);
    }
}
