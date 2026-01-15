using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IMaterialEPPService
    {
        Task<IEnumerable<MaterialEPP>> GetAllMaterialesEPPAsync();
        Task<MaterialEPP?> GetMaterialEPPByIdAsync(int id);
        Task<MaterialEPP> CreateMaterialEPPAsync(MaterialEPP material);
        Task<bool> UpdateMaterialEPPAsync(MaterialEPP material);
        Task<bool> DeleteMaterialEPPAsync(int id);
    }
}
