using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetAll();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> SaveChangesAsync();
        Task ReloadAsync(T entity);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,Func<IQueryable<T>, IQueryable<T>>? include = null);

        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,Func<IQueryable<T>, IQueryable<T>>? include = null);

        Task<(List<T> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
