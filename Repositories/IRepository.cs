using System.Linq.Expressions;
using GraphQLApi.Models;

namespace GraphQLApi.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync();
    Task<bool> ExistsAsync(int id);
}

public interface IBookRepository : IRepository<Book>
{
    Task<IEnumerable<Book>> GetByAuthorIdAsync(int authorId);
    Task<IEnumerable<Book>> GetAvailableBooksAsync();
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
}

public interface IAuthorRepository : IRepository<Author>
{
    Task<Author?> GetByEmailAsync(string email);
    Task<bool> HasBooksAsync(int authorId);
}