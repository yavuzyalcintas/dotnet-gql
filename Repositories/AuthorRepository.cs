using GraphQLApi.Data;
using GraphQLApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GraphQLApi.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly AuthorDbContext _context;
    private readonly BookDbContext _bookContext;

    public AuthorRepository(AuthorDbContext context, BookDbContext bookContext)
    {
        _context = context;
        _bookContext = bookContext;
    }

    public IQueryable<Author> GetAll() => _context.Authors.AsQueryable();

    public async Task<Author?> GetByIdAsync(int id)
    {
        return await _context.Authors.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Author>> FindAsync(Expression<Func<Author, bool>> predicate)
    {
        return await _context.Authors.Where(predicate).ToListAsync();
    }

    public async Task<Author> AddAsync(Author entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Authors.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Author> UpdateAsync(Author entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Authors.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == id);
        if (author == null) return false;

        // Check if author has books (cross-database check)
        var hasBooks = await HasBooksAsync(id);
        if (hasBooks)
        {
            throw new InvalidOperationException("Cannot delete author with existing books.");
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAsync() => await _context.Authors.CountAsync();

    public async Task<bool> ExistsAsync(int id) => await _context.Authors.AnyAsync(a => a.Id == id);

    // Author-specific methods
    public async Task<Author?> GetByEmailAsync(string email)
    {
        return await _context.Authors.FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<bool> HasBooksAsync(int authorId)
    {
        return await _bookContext.Books.AnyAsync(b => b.AuthorId == authorId);
    }
}