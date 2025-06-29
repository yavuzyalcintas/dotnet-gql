using GraphQLApi.Models;
using GraphQLApi.Services;

namespace GraphQLApi.GraphQL.Resolvers;

[ExtendObjectType<Author>]
public class AuthorResolvers
{
    /// <summary>
    /// Resolver to get all books for an author from the separate Book database
    /// </summary>
    public async Task<IEnumerable<Book>> GetBooks([Parent] Author author, [Service] BookService bookService)
    {
        return await bookService.GetBooksByAuthorIdAsync(author.Id);
    }

    /// <summary>
    /// Resolver to get count of books for an author
    /// </summary>
    public async Task<int> GetBooksCount([Parent] Author author, [Service] BookService bookService)
    {
        return await bookService.GetBooksCountForAuthorAsync(author.Id);
    }

    /// <summary>
    /// Resolver to get available books only
    /// </summary>
    public async Task<IEnumerable<Book>> GetAvailableBooks([Parent] Author author, [Service] BookService bookService)
    {
        var books = await bookService.GetBooksByAuthorIdAsync(author.Id);
        return books.Where(b => b.IsAvailable);
    }

    /// <summary>
    /// Resolver to get total value of all books by this author
    /// </summary>
    public async Task<decimal> GetTotalBooksValue([Parent] Author author, [Service] BookService bookService)
    {
        var books = await bookService.GetBooksByAuthorIdAsync(author.Id);
        return books.Sum(b => b.Price);
    }

    /// <summary>
    /// Resolver to get the most expensive book by this author
    /// </summary>
    public async Task<Book?> GetMostExpensiveBook([Parent] Author author, [Service] BookService bookService)
    {
        var books = await bookService.GetBooksByAuthorIdAsync(author.Id);
        return books.OrderByDescending(b => b.Price).FirstOrDefault();
    }

    /// <summary>
    /// Resolver to get author's age
    /// </summary>
    public int GetAge([Parent] Author author)
    {
        return DateTime.Now.Year - author.DateOfBirth.Year;
    }

    /// <summary>
    /// Resolver to get years since first publication
    /// </summary>
    public async Task<int?> GetYearsSinceFirstPublication([Parent] Author author, [Service] BookService bookService)
    {
        var books = await bookService.GetBooksByAuthorIdAsync(author.Id);
        var earliestBook = books.OrderBy(b => b.PublishedDate).FirstOrDefault();

        if (earliestBook == null) return null;

        return DateTime.Now.Year - earliestBook.PublishedDate.Year;
    }
}