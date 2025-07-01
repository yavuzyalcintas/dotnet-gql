using GraphQLApi.Models;
using GraphQLApi.Repositories;
using GraphQLApi.ExternalServices;

namespace GraphQLApi.Services;

public class BookDomainService
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IInventoryApiClient _inventoryClient;
    private readonly ILogger<BookDomainService> _logger;

    public BookDomainService(
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        IInventoryApiClient inventoryClient,
        ILogger<BookDomainService> logger)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _inventoryClient = inventoryClient;
        _logger = logger;
    }

    // Query operations
    public IQueryable<Book> GetBooks() => _bookRepository.GetAll();

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _bookRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Book>> GetBooksByAuthorIdAsync(int authorId)
    {
        return await _bookRepository.GetByAuthorIdAsync(authorId);
    }

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
    {
        return await _bookRepository.GetAvailableBooksAsync();
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        return await _bookRepository.SearchBooksAsync(searchTerm);
    }

    // Command operations with business logic
    public async Task<Book> AddBookAsync(string title, string description, int authorId, decimal price)
    {
        // Validate author exists
        var authorExists = await _authorRepository.ExistsAsync(authorId);
        if (!authorExists)
            throw new ArgumentException("Author not found");

        // Validate business rules
        if (price < 0)
            throw new ArgumentException("Price cannot be negative");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required");

        var book = new Book
        {
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            AuthorId = authorId,
            PublishedDate = DateTime.UtcNow,
            Price = price,
            IsAvailable = true
        };

        var savedBook = await _bookRepository.AddAsync(book);

        _logger.LogInformation("Book created: {BookId} - {Title}", savedBook.Id, savedBook.Title);

        return savedBook;
    }

    public async Task<Book?> UpdateBookAsync(int id, string? title = null, string? description = null,
        decimal? price = null, bool? isAvailable = null)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return null;

        // Validate updates
        if (price.HasValue && price.Value < 0)
            throw new ArgumentException("Price cannot be negative");

        if (!string.IsNullOrWhiteSpace(title))
            book.Title = title.Trim();

        if (description != null)
            book.Description = description.Trim();

        if (price.HasValue)
            book.Price = price.Value;

        if (isAvailable.HasValue)
            book.IsAvailable = isAvailable.Value;

        var updatedBook = await _bookRepository.UpdateAsync(book);

        _logger.LogInformation("Book updated: {BookId} - {Title}", updatedBook.Id, updatedBook.Title);

        return updatedBook;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return false;

        var deleted = await _bookRepository.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation("Book deleted: {BookId} - {Title}", id, book.Title);
        }

        return deleted;
    }

    // Cross-domain operations
    public async Task<Author?> GetAuthorForBookAsync(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null) return null;

        return await _authorRepository.GetByIdAsync(book.AuthorId);
    }

    // External API integration
    public async Task<InventoryItem?> GetBookInventoryAsync(int bookId)
    {
        return await _inventoryClient.GetInventoryByBookIdAsync(bookId);
    }

    public async Task<bool> UpdateBookStockAsync(int bookId, int quantity)
    {
        // Verify book exists
        var bookExists = await _bookRepository.ExistsAsync(bookId);
        if (!bookExists) return false;

        return await _inventoryClient.UpdateStockAsync(bookId, quantity);
    }

    // Business analytics
    public async Task<int> GetTotalBooksCountAsync()
    {
        return await _bookRepository.CountAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksWithLowStockAsync()
    {
        var lowStockItems = await _inventoryClient.GetLowStockItemsAsync();
        var bookIds = lowStockItems.Select(item => item.BookId).ToList();

        if (!bookIds.Any()) return Enumerable.Empty<Book>();

        return await _bookRepository.FindAsync(book => bookIds.Contains(book.Id));
    }
}