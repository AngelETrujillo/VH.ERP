using VH.Services.Entities;
using VH.Services.Interfaces;

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
            // Validar RFC duplicado
            var existente = await _unitOfWork.Proveedores.FindAsync(
                p => p.RFC.ToUpper() == proveedor.RFC.ToUpper());

            if (existente.Any())
                throw new InvalidOperationException($"Ya existe un proveedor con el RFC '{proveedor.RFC}'.");

            await _unitOfWork.Proveedores.AddAsync(proveedor);
            await _unitOfWork.CompleteAsync();
            return proveedor;
        }

        public async Task<bool> UpdateProveedorAsync(Proveedor proveedor)
        {
            var proveedorExistente = await _unitOfWork.Proveedores.GetByIdAsync(proveedor.IdProveedor);
            if (proveedorExistente == null)
                return false;

            // Validar RFC duplicado (excluyendo el actual)
            var duplicado = await _unitOfWork.Proveedores.FindAsync(
                p => p.IdProveedor != proveedor.IdProveedor &&
                     p.RFC.ToUpper() == proveedor.RFC.ToUpper());

            if (duplicado.Any())
                throw new InvalidOperationException($"Ya existe otro proveedor con el RFC '{proveedor.RFC}'.");

            proveedorExistente.Nombre = proveedor.Nombre;
            proveedorExistente.RFC = proveedor.RFC;
            proveedorExistente.Contacto = proveedor.Contacto;
            proveedorExistente.Telefono = proveedor.Telefono;
            proveedorExistente.Activo = proveedor.Activo;

            _unitOfWork.Proveedores.Update(proveedorExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteProveedorAsync(int id)
        {
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id);
            if (proveedor == null)
                return false;

            // Validar que no tenga compras asociadas
            var compras = await _unitOfWork.ComprasEPP.FindAsync(c => c.IdProveedor == id);
            if (compras.Any())
                throw new InvalidOperationException("No se puede eliminar el proveedor porque tiene compras registradas.");

            _unitOfWork.Proveedores.Remove(proveedor);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}