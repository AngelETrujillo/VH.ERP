using VH.Services.Entities;
using VH.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VH.Services.Services
{
    public class AlmacenService : IAlmacenService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlmacenService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Almacen>> GetAllAlmacenesAsync()
        {
            return await _unitOfWork.Almacenes.GetAllAsync(includeProperties: "Proyecto");
        }

        public async Task<IEnumerable<Almacen>> GetAlmacenesByProyectoAsync(int idProyecto)
        {
            return await _unitOfWork.Almacenes.FindAsync(
                filter: a => a.IdProyecto == idProyecto,
                includeProperties: "Proyecto"
            );
        }

        public async Task<Almacen?> GetAlmacenByIdAsync(int id)
        {
            return await _unitOfWork.Almacenes.GetByIdAsync(id, includeProperties: "Proyecto,Inventarios");
        }

        public async Task<Almacen> CreateAlmacenAsync(Almacen almacen)
        {
            // Validar que el proyecto exista
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(almacen.IdProyecto);
            if (proyecto == null)
            {
                throw new ArgumentException($"El proyecto con ID {almacen.IdProyecto} no existe.");
            }

            // Validar que no exista un almacén con el mismo nombre en el mismo proyecto
            var almacenExistente = await _unitOfWork.Almacenes.FindAsync(
                a => a.IdProyecto == almacen.IdProyecto &&
                     a.Nombre.ToLower() == almacen.Nombre.ToLower()
            );

            if (almacenExistente.Any())
            {
                throw new InvalidOperationException($"Ya existe un almacén con el nombre '{almacen.Nombre}' en este proyecto.");
            }

            await _unitOfWork.Almacenes.AddAsync(almacen);
            await _unitOfWork.CompleteAsync();
            return almacen;
        }

        public async Task<bool> UpdateAlmacenAsync(Almacen almacen)
        {
            var almacenExistente = await _unitOfWork.Almacenes.GetByIdAsync(almacen.IdAlmacen);
            if (almacenExistente == null)
            {
                return false;
            }

            // Validar que el proyecto exista (si se está cambiando)
            if (almacen.IdProyecto != almacenExistente.IdProyecto)
            {
                var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(almacen.IdProyecto);
                if (proyecto == null)
                {
                    return false;
                }
            }

            // Validar que no exista otro almacén con el mismo nombre en el mismo proyecto
            var duplicado = await _unitOfWork.Almacenes.FindAsync(
                a => a.IdAlmacen != almacen.IdAlmacen &&
                     a.IdProyecto == almacen.IdProyecto &&
                     a.Nombre.ToLower() == almacen.Nombre.ToLower()
            );

            if (duplicado.Any())
            {
                return false;
            }

            // Actualizar campos
            almacenExistente.Nombre = almacen.Nombre;
            almacenExistente.Descripcion = almacen.Descripcion;
            almacenExistente.Domicilio = almacen.Domicilio;
            almacenExistente.TipoUbicacion = almacen.TipoUbicacion;
            almacenExistente.Activo = almacen.Activo;
            almacenExistente.IdProyecto = almacen.IdProyecto;

            _unitOfWork.Almacenes.Update(almacenExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteAlmacenAsync(int id)
        {
            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(id);
            if (almacen == null)
            {
                return false;
            }

            // Verificar si tiene registros de inventario
            var inventarios = await _unitOfWork.Inventarios.FindAsync(i => i.IdAlmacen == id);
            if (inventarios.Any())
            {
                throw new InvalidOperationException("No se puede eliminar el almacén porque tiene registros de inventario asociados.");
            }

            _unitOfWork.Almacenes.Remove(almacen);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}