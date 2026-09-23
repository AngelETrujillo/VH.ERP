using System.Linq.Expressions;
using VH.Services.DTOs;
using VH.Services.Entities; // Necesario para Tipos de Entidad

namespace VH.Services.Interfaces 
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        Task<T?> GetByIdAsync(int id, string? includeProperties = null);

        /// <summary>
        /// Una página de resultados, recortada en la base y no en memoria.
        ///
        /// El resto de los métodos materializan la tabla entera antes de que nadie
        /// pueda recortarla; con el kardex creciendo cada día eso deja de ser viable.
        /// Aquí el Skip y el Take viajan a SQL, y el total sale de un COUNT sobre el
        /// mismo filtro.
        /// </summary>
        /// <param name="orden">
        /// Se recibe como función sobre el IQueryable y no como expresión de campo,
        /// porque ordenar por un tipo valor obligaría a encajonarlo y EF ya no sabría
        /// traducirlo. Así también se puede ordenar por varias columnas.
        /// </param>
        Task<ResultadoPaginado<T>> GetPaginadoAsync(
            ConsultaPaginada consulta,
            Expression<Func<T, bool>>? filtro = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orden = null,
            string? includeProperties = null);

        /// <summary>Cuántos hay que cumplan el filtro, sin traer ninguno.</summary>
        Task<int> ContarAsync(Expression<Func<T, bool>>? filtro = null);
    }
}