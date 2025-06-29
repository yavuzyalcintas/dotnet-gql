using GraphQLApi.Models;
using GraphQLApi.Services;
using Microsoft.EntityFrameworkCore;

namespace GraphQLApi.GraphQL.Resolvers;

[ExtendObjectType<Mutation>]
public class BookMutationResolvers
{
    /// <summary>
    /// Add a new book
    /// </summary>
    public async Task<Book> AddBook([Service] BookService bookService, string title, string description, int authorId, decimal price)
    {
        return await bookService.AddBookAsync(title, description, authorId, price);
    }

    /// <summary>
    /// Update an existing book
    /// </summary>
    public async Task<Book?> UpdateBook([Service] BookService bookService, int id, string? title = null, string? description = null, decimal? price = null, bool? isAvailable = null)
    {
        return await bookService.UpdateBookAsync(id, title, description, price, isAvailable);
    }

    /// <summary>
    /// Delete a book
    /// </summary>
    public async Task<bool> DeleteBook([Service] BookService bookService, int id)
    {
        return await bookService.DeleteBookAsync(id);
    }

    /// <summary>
    /// Toggle book availability
    /// </summary>
    public async Task<Book?> ToggleBookAvailability([Service] BookService bookService, int id)
    {
        var book = await bookService.GetBookByIdAsync(id);
        if (book == null) return null;

        return await bookService.UpdateBookAsync(id, isAvailable: !book.IsAvailable);
    }

    /// <summary>
    /// Bulk update book prices by percentage
    /// </summary>
    public async Task<IEnumerable<Book>> BulkUpdateBookPrices([Service] BookService bookService, decimal percentageChange)
    {
        var books = await bookService.GetBooks().ToListAsync();
        var updatedBooks = new List<Book>();

        foreach (var book in books)
        {
            var newPrice = book.Price * (1 + percentageChange / 100);
            var updatedBook = await bookService.UpdateBookAsync(book.Id, price: newPrice);
            if (updatedBook != null)
                updatedBooks.Add(updatedBook);
        }

        return updatedBooks;
    }

    /// <summary>
    /// Mark books as unavailable by author
    /// </summary>
    public async Task<IEnumerable<Book>> MarkBooksUnavailableByAuthor([Service] BookService bookService, int authorId)
    {
        var books = await bookService.GetBooksByAuthorIdAsync(authorId);
        var updatedBooks = new List<Book>();

        foreach (var book in books)
        {
            var updatedBook = await bookService.UpdateBookAsync(book.Id, isAvailable: false);
            if (updatedBook != null)
                updatedBooks.Add(updatedBook);
        }

        return updatedBooks;
    }

    /// <summary>
    /// Update book with validation
    /// </summary>
    public async Task<Book?> UpdateBookWithValidation([Service] BookService bookService, int id, string? title = null, string? description = null, decimal? price = null, bool? isAvailable = null)
    {
        // Validate price if provided
        if (price.HasValue && price.Value < 0)
        {
            throw new ArgumentException("Price cannot be negative");
        }

        // Validate title if provided
        if (!string.IsNullOrEmpty(title) && title.Length > 200)
        {
            throw new ArgumentException("Title cannot exceed 200 characters");
        }

        return await bookService.UpdateBookAsync(id, title, description, price, isAvailable);
    }
}