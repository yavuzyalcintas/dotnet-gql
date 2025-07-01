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
    public IQueryable<Book> GetBooks([Service] BookDomainService bookService) => bookService.GetBooks();

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Author> GetAuthors([Service] AuthorDomainService authorService) => authorService.GetAuthors();

    public async Task<Book?> GetBook([Service] BookDomainService bookService, int id) =>
        await bookService.GetBookByIdAsync(id);

    public async Task<Author?> GetAuthor([Service] AuthorDomainService authorService, int id) =>
        await authorService.GetAuthorByIdAsync(id);

    // Convenience queries for common filtering
    public async Task<IEnumerable<Book>> GetAvailableBooks([Service] BookDomainService bookService) =>
        await bookService.GetAvailableBooksAsync();

    public async Task<IEnumerable<Book>> SearchBooks([Service] BookDomainService bookService, string searchTerm) =>
        await bookService.SearchBooksAsync(searchTerm);

    public async Task<IEnumerable<Book>> GetBooksByAuthor([Service] BookDomainService bookService, int authorId) =>
        await bookService.GetBooksByAuthorIdAsync(authorId);

    // Statistics queries
    public async Task<int> GetTotalBooksCount([Service] BookDomainService bookService) =>
        await bookService.GetTotalBooksCountAsync();

    public async Task<int> GetTotalAuthorsCount([Service] AuthorDomainService authorService) =>
        await authorService.GetTotalAuthorsCountAsync();

    public async Task<decimal> GetAverageBookPrice([Service] BookDomainService bookService) =>
        await bookService.GetBooks().AverageAsync(b => b.Price);

    public async Task<Book?> GetMostExpensiveBook([Service] BookDomainService bookService) =>
        await bookService.GetBooks().OrderByDescending(b => b.Price).FirstOrDefaultAsync();

    public async Task<Book?> GetNewestBook([Service] BookDomainService bookService) =>
        await bookService.GetBooks().OrderByDescending(b => b.PublishedDate).FirstOrDefaultAsync();

    // External API integrated queries
    public async Task<IEnumerable<Book>> GetBooksWithLowStock([Service] BookDomainService bookService) =>
        await bookService.GetBooksWithLowStockAsync();

    // Cross-domain queries
    public async Task<IEnumerable<Author>> GetAuthorsWithoutBooks([Service] AuthorDomainService authorService) =>
        await authorService.GetAuthorsWithoutBooksAsync();
}