using GraphQLApi.Models;
using GraphQLApi.Services;
using Microsoft.EntityFrameworkCore;

namespace GraphQLApi.GraphQL;

public class Query
{
    // Basic queries - GraphQL resolvers will handle cross-database relationships automatically

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Book> GetBooks([Service] BookService bookService) => bookService.GetBooks();

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Author> GetAuthors([Service] BookService bookService) => bookService.GetAuthors();

    public async Task<Book?> GetBook([Service] BookService bookService, int id) =>
        await bookService.GetBookByIdAsync(id);

    public async Task<Author?> GetAuthor([Service] BookService bookService, int id) =>
        await bookService.GetAuthorByIdAsync(id);

    // Convenience queries for common filtering
    public async Task<IEnumerable<Book>> GetAvailableBooks([Service] BookService bookService) =>
        await bookService.GetAvailableBooksAsync();

    public async Task<IEnumerable<Book>> SearchBooks([Service] BookService bookService, string searchTerm) =>
        await bookService.SearchBooksAsync(searchTerm);

    public async Task<IEnumerable<Book>> GetBooksByAuthor([Service] BookService bookService, int authorId) =>
        await bookService.GetBooksByAuthorIdAsync(authorId);

    // Statistics queries
    public async Task<int> GetTotalBooksCount([Service] BookService bookService) =>
        await bookService.GetBooks().CountAsync();

    public async Task<int> GetTotalAuthorsCount([Service] BookService bookService) =>
        await bookService.GetAuthors().CountAsync();

    public async Task<decimal> GetAverageBookPrice([Service] BookService bookService) =>
        await bookService.GetBooks().AverageAsync(b => b.Price);

    public async Task<Book?> GetMostExpensiveBook([Service] BookService bookService) =>
        await bookService.GetBooks().OrderByDescending(b => b.Price).FirstOrDefaultAsync();

    public async Task<Book?> GetNewestBook([Service] BookService bookService) =>
        await bookService.GetBooks().OrderByDescending(b => b.PublishedDate).FirstOrDefaultAsync();
}