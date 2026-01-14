using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IProyectoEPPReaderService
    {
        Task<Proyecto> GetProyectoByIdAsync(int id);
    }
}
