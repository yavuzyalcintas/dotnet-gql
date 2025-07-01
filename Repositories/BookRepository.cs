using GraphQLApi.Data;
using GraphQLApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GraphQLApi.Repositories;

public class BookRepository : IBookRepository
{
    private readonly BookDbContext _context;

    public BookRepository(BookDbContext context)
    {
        _context = context;
    }

    public IQueryable<Book> GetAll() => _context.Books.AsQueryable();

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Book>> FindAsync(Expression<Func<Book, bool>> predicate)
    {
        return await _context.Books.Where(predicate).ToListAsync();
    }

    public async Task<Book> AddAsync(Book entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Books.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Book> UpdateAsync(Book entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Books.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAsync() => await _context.Books.CountAsync();

    public async Task<bool> ExistsAsync(int id) => await _context.Books.AnyAsync(b => b.Id == id);

    // Book-specific methods
    public async Task<IEnumerable<Book>> GetByAuthorIdAsync(int authorId)
    {
        return await _context.Books.Where(b => b.AuthorId == authorId).ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
    {
        return await _context.Books.Where(b => b.IsAvailable).ToListAsync();
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        return await _context.Books
            .Where(b => b.Title.Contains(searchTerm) || b.Description.Contains(searchTerm))
            .ToListAsync();
    }
}