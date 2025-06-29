using GraphQLApi.Models;
using GraphQLApi.Data;
using Microsoft.EntityFrameworkCore;

namespace GraphQLApi.Services;

public class BookService
{
    private readonly BookDbContext _bookContext;
    private readonly AuthorDbContext _authorContext;

    public BookService(BookDbContext bookContext, AuthorDbContext authorContext)
    {
        _bookContext = bookContext;
        _authorContext = authorContext;
    }

    // Book operations
    public IQueryable<Book> GetBooks() => _bookContext.Books.AsQueryable();

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _bookContext.Books.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Book>> GetBooksByAuthorIdAsync(int authorId)
    {
        return await _bookContext.Books
            .Where(b => b.AuthorId == authorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
    {
        return await _bookContext.Books
            .Where(b => b.IsAvailable)
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        return await _bookContext.Books
            .Where(b => b.Title.Contains(searchTerm) || b.Description.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<Book> AddBookAsync(string title, string description, int authorId, decimal price)
    {
        // Verify author exists
        var authorExists = await _authorContext.Authors.AnyAsync(a => a.Id == authorId);
        if (!authorExists)
            throw new ArgumentException("Author not found");

        var book = new Book
        {
            Title = title,
            Description = description,
            AuthorId = authorId,
            PublishedDate = DateTime.UtcNow,
            Price = price,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _bookContext.Books.Add(book);
        await _bookContext.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateBookAsync(int id, string? title = null, string? description = null, decimal? price = null, bool? isAvailable = null)
    {
        var book = await _bookContext.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return null;

        if (title != null) book.Title = title;
        if (description != null) book.Description = description;
        if (price.HasValue) book.Price = price.Value;
        if (isAvailable.HasValue) book.IsAvailable = isAvailable.Value;

        book.UpdatedAt = DateTime.UtcNow;

        await _bookContext.SaveChangesAsync();
        return book;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _bookContext.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return false;

        _bookContext.Books.Remove(book);
        await _bookContext.SaveChangesAsync();
        return true;
    }

    // Author operations
    public IQueryable<Author> GetAuthors() => _authorContext.Authors.AsQueryable();

    public async Task<Author?> GetAuthorByIdAsync(int id)
    {
        return await _authorContext.Authors.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Author> AddAuthorAsync(string name, string email, DateTime dateOfBirth)
    {
        var author = new Author
        {
            Name = name,
            Email = email,
            DateOfBirth = dateOfBirth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _authorContext.Authors.Add(author);
        await _authorContext.SaveChangesAsync();
        return author;
    }

    public async Task<Author?> UpdateAuthorAsync(int id, string? name = null, string? email = null, DateTime? dateOfBirth = null)
    {
        var author = await _authorContext.Authors.FirstOrDefaultAsync(a => a.Id == id);
        if (author == null) return null;

        if (name != null) author.Name = name;
        if (email != null) author.Email = email;
        if (dateOfBirth.HasValue) author.DateOfBirth = dateOfBirth.Value;

        author.UpdatedAt = DateTime.UtcNow;

        await _authorContext.SaveChangesAsync();
        return author;
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var author = await _authorContext.Authors.FirstOrDefaultAsync(a => a.Id == id);
        if (author == null) return false;

        // Check if author has books
        var hasBooks = await _bookContext.Books.AnyAsync(b => b.AuthorId == id);
        if (hasBooks)
        {
            throw new InvalidOperationException("Cannot delete author with existing books. Please delete or reassign the books first.");
        }

        _authorContext.Authors.Remove(author);
        await _authorContext.SaveChangesAsync();
        return true;
    }

    // Helper method to get author details for a book (cross-database query)
    public async Task<Author?> GetAuthorForBookAsync(int bookId)
    {
        var book = await _bookContext.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        if (book == null) return null;

        return await _authorContext.Authors.FirstOrDefaultAsync(a => a.Id == book.AuthorId);
    }

    // Helper method to get books count for an author
    public async Task<int> GetBooksCountForAuthorAsync(int authorId)
    {
        return await _bookContext.Books.CountAsync(b => b.AuthorId == authorId);
    }
}