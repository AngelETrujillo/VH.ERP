using VH.Services.Entities;
using VH.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VH.Services.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProveedorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Proveedor>> GetAllProveedoresAsync()
        {
            return await _unitOfWork.Proveedores.GetAllAsync();
        }

        public async Task<Proveedor?> GetProveedorByIdAsync(int id)
        {
            return await _unitOfWork.Proveedores.GetByIdAsync(id);
        }

        public async Task<Proveedor> CreateProveedorAsync(Proveedor proveedor)
        {
            await _unitOfWork.Proveedores.AddAsync(proveedor);
            await _unitOfWork.CompleteAsync();
            return proveedor;
        }

        public async Task<bool> UpdateProveedorAsync(Proveedor proveedor)
        {
            _unitOfWork.Proveedores.Update(proveedor);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteProveedorAsync(int id)
        {
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id);
            if (proveedor == null) return false;

            _unitOfWork.Proveedores.Remove(proveedor);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}