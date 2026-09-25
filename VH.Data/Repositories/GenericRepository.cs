using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VH.Services.Interfaces;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Data;

namespace VH.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly VHERPContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(VHERPContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        // 1. GetAllAsync con soporte para Include
        public async Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;

            if (includeProperties != null)
            {
                // Divide la cadena "Prop1,Prop2" y aplica cada Include
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<T?> GetByIdAsync(int id, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            var keyName = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.Select(x => x.Name).Single();

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, keyName ?? "Id") == id);
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<ResultadoPaginado<T>> GetPaginadoAsync(
            ConsultaPaginada consulta,
            Expression<Func<T, bool>>? filtro = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orden = null,
            string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;

            if (filtro != null)
                query = query.Where(filtro);

            // El total se cuenta antes de los Include: contar no necesita traer las
            // tablas relacionadas, y con ellas el COUNT se vuelve caro sin motivo.
            var total = await query.CountAsync();

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(
                             new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            // Sin un orden estable, saltar renglones no significa nada: la base puede
            // devolverlos en distinto orden en cada consulta y una misma fila aparecer
            // en dos páginas o en ninguna.
            query = orden != null
                ? orden(query)
                : OrdenarPorLlave(query);

            var renglones = await query
                .Skip(consulta.Salto)
                .Take(consulta.Tamano)
                .ToListAsync();

            return new ResultadoPaginado<T>
            {
                Renglones = renglones,
                Total = total,
                Pagina = consulta.Pagina,
                Tamano = consulta.Tamano,
                Buscado = consulta.TextoLimpio
            };
        }

        public async Task<int> ContarAsync(Expression<Func<T, bool>>? filtro = null)
        {
            IQueryable<T> query = _dbSet;
            if (filtro != null) query = query.Where(filtro);
            return await query.CountAsync();
        }

        /// <summary>
        /// Orden de respaldo cuando quien llama no indicó ninguno: la llave primaria,
        /// descendente, que en estas tablas es lo más reciente primero.
        /// </summary>
        private IOrderedQueryable<T> OrdenarPorLlave(IQueryable<T> query)
        {
            var llave = _context.Model.FindEntityType(typeof(T))
                ?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;

            return llave == null
                ? query.OrderBy(e => 0)          // sin llave declarada; al menos es estable
                : query.OrderByDescending(e => EF.Property<object>(e, llave));
        }

        public void Update(T entity)
        {
            // Adjunta la entidad si no está siendo rastreada y marca como modificada.
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(T entity) => _dbSet.Remove(entity);
    }
}