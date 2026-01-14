using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class ProyectoEPPReaderService : IProyectoEPPReaderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProyectoEPPReaderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Proyecto> GetProyectoByIdAsync(int id)
        {
            // Usa el repositorio de Proyectos existente
            return await _unitOfWork.Proyectos.GetByIdAsync(id);
        }
    }
}