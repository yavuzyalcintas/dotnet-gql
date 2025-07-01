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
    public async Task<Book> AddBook([Service] BookDomainService bookService, string title, string description, int authorId, decimal price)
    {
        return await bookService.AddBookAsync(title, description, authorId, price);
    }

    /// <summary>
    /// Update an existing book
    /// </summary>
    public async Task<Book?> UpdateBook([Service] BookDomainService bookService, int id, string? title = null, string? description = null, decimal? price = null, bool? isAvailable = null)
    {
        return await bookService.UpdateBookAsync(id, title, description, price, isAvailable);
    }

    /// <summary>
    /// Delete a book
    /// </summary>
    public async Task<bool> DeleteBook([Service] BookDomainService bookService, int id)
    {
        return await bookService.DeleteBookAsync(id);
    }

    /// <summary>
    /// Toggle book availability
    /// </summary>
    public async Task<Book?> ToggleBookAvailability([Service] BookDomainService bookService, int id)
    {
        var book = await bookService.GetBookByIdAsync(id);
        if (book == null) return null;

        return await bookService.UpdateBookAsync(id, isAvailable: !book.IsAvailable);
    }

    /// <summary>
    /// Bulk update book prices by percentage
    /// </summary>
    public async Task<IEnumerable<Book>> BulkUpdateBookPrices([Service] BookDomainService bookService, decimal percentageChange)
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
    public async Task<IEnumerable<Book>> MarkBooksUnavailableByAuthor([Service] BookDomainService bookService, int authorId)
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
    public async Task<Book?> UpdateBookWithValidation([Service] BookDomainService bookService, int id, string? title = null, string? description = null, decimal? price = null, bool? isAvailable = null)
    {
        // The domain service now handles validation, so we can simplify this
        return await bookService.UpdateBookAsync(id, title, description, price, isAvailable);
    }

    /// <summary>
    /// Update book stock via external inventory API
    /// </summary>
    public async Task<bool> UpdateBookStock([Service] BookDomainService bookService, int bookId, int quantity)
    {
        return await bookService.UpdateBookStockAsync(bookId, quantity);
    }
}