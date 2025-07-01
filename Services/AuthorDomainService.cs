using GraphQLApi.Models;
using GraphQLApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GraphQLApi.Services;

public class AuthorDomainService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<AuthorDomainService> _logger;

    public AuthorDomainService(
        IAuthorRepository authorRepository,
        IBookRepository bookRepository,
        ILogger<AuthorDomainService> logger)
    {
        _authorRepository = authorRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }

    // Query operations
    public IQueryable<Author> GetAuthors() => _authorRepository.GetAll();

    public async Task<Author?> GetAuthorByIdAsync(int id)
    {
        return await _authorRepository.GetByIdAsync(id);
    }

    public async Task<Author?> GetAuthorByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");

        return await _authorRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());
    }

    // Command operations with business logic
    public async Task<Author> AddAuthorAsync(string name, string email, DateTime dateOfBirth)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");

        if (dateOfBirth > DateTime.Today)
            throw new ArgumentException("Date of birth cannot be in the future");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // Check for duplicate email
        var existingAuthor = await _authorRepository.GetByEmailAsync(normalizedEmail);
        if (existingAuthor != null)
            throw new InvalidOperationException("An author with this email already exists");

        var author = new Author
        {
            Name = name.Trim(),
            Email = normalizedEmail,
            DateOfBirth = dateOfBirth
        };

        var savedAuthor = await _authorRepository.AddAsync(author);

        _logger.LogInformation("Author created: {AuthorId} - {Name}", savedAuthor.Id, savedAuthor.Name);

        return savedAuthor;
    }

    public async Task<Author?> UpdateAuthorAsync(int id, string? name = null, string? email = null, DateTime? dateOfBirth = null)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        if (author == null) return null;

        // Validate updates
        if (dateOfBirth.HasValue && dateOfBirth.Value > DateTime.Today)
            throw new ArgumentException("Date of birth cannot be in the future");

        if (!string.IsNullOrWhiteSpace(name))
            author.Name = name.Trim();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            // Check for duplicate email (excluding current author)
            var existingAuthor = await _authorRepository.GetByEmailAsync(normalizedEmail);
            if (existingAuthor != null && existingAuthor.Id != id)
                throw new InvalidOperationException("Another author with this email already exists");

            author.Email = normalizedEmail;
        }

        if (dateOfBirth.HasValue)
            author.DateOfBirth = dateOfBirth.Value;

        var updatedAuthor = await _authorRepository.UpdateAsync(author);

        _logger.LogInformation("Author updated: {AuthorId} - {Name}", updatedAuthor.Id, updatedAuthor.Name);

        return updatedAuthor;
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        if (author == null) return false;

        // Check if author has books
        var hasBooks = await _authorRepository.HasBooksAsync(id);
        if (hasBooks)
        {
            throw new InvalidOperationException("Cannot delete author with existing books. Please delete or reassign the books first.");
        }

        var deleted = await _authorRepository.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation("Author deleted: {AuthorId} - {Name}", id, author.Name);
        }

        return deleted;
    }

    // Cross-domain operations
    public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
    {
        var authorExists = await _authorRepository.ExistsAsync(authorId);
        if (!authorExists) return Enumerable.Empty<Book>();

        return await _bookRepository.GetByAuthorIdAsync(authorId);
    }

    public async Task<int> GetBooksCountForAuthorAsync(int authorId)
    {
        var books = await GetBooksByAuthorAsync(authorId);
        return books.Count();
    }

    // Business analytics
    public async Task<int> GetTotalAuthorsCountAsync()
    {
        return await _authorRepository.CountAsync();
    }

    public async Task<decimal> GetTotalBooksValueForAuthorAsync(int authorId)
    {
        var books = await GetBooksByAuthorAsync(authorId);
        return books.Sum(book => book.Price);
    }

    public async Task<IEnumerable<Author>> GetAuthorsWithoutBooksAsync()
    {
        var allAuthors = await _authorRepository.GetAll().ToListAsync();
        var authorsWithoutBooks = new List<Author>();

        foreach (var author in allAuthors)
        {
            var hasBooks = await _authorRepository.HasBooksAsync(author.Id);
            if (!hasBooks)
            {
                authorsWithoutBooks.Add(author);
            }
        }

        return authorsWithoutBooks;
    }
}