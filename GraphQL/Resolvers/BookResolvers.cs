using GraphQLApi.Models;
using GraphQLApi.Services;

namespace GraphQLApi.GraphQL.Resolvers;

[ExtendObjectType<Book>]
public class BookResolvers
{
    /// <summary>
    /// Resolver to get the author for a book from the separate Author database
    /// </summary>
    public async Task<Author?> GetAuthor([Parent] Book book, [Service] BookService bookService)
    {
        return await bookService.GetAuthorByIdAsync(book.AuthorId);
    }

    /// <summary>
    /// Resolver to get formatted author name
    /// </summary>
    public async Task<string?> GetAuthorName([Parent] Book book, [Service] BookService bookService)
    {
        var author = await bookService.GetAuthorByIdAsync(book.AuthorId);
        return author?.Name;
    }

    /// <summary>
    /// Resolver to check if the book's author exists
    /// </summary>
    public async Task<bool> GetHasValidAuthor([Parent] Book book, [Service] BookService bookService)
    {
        var author = await bookService.GetAuthorByIdAsync(book.AuthorId);
        return author != null;
    }

    /// <summary>
    /// Resolver to get formatted price with currency
    /// </summary>
    public string GetFormattedPrice([Parent] Book book)
    {
        return $"${book.Price:F2}";
    }

    /// <summary>
    /// Resolver to calculate age of the book
    /// </summary>
    public int GetAgeInYears([Parent] Book book)
    {
        return DateTime.Now.Year - book.PublishedDate.Year;
    }
}