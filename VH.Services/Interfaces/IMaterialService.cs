using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IMaterialService
    {
        /// <summary>
        /// Catálogo completo, o sólo los de un tipo. El filtro por tipo es lo que
        /// permite que la configuración de alertas ofrezca únicamente EPP.
        /// </summary>
        Task<IEnumerable<Material>> GetAllMaterialesAsync(TipoMaterial? tipo = null);
        Task<Material?> GetMaterialByIdAsync(int id);
        Task<Material> CreateMaterialAsync(Material material);
        Task<bool> UpdateMaterialAsync(Material material);
        Task<bool> DeleteMaterialAsync(int id);
    }
}
