using GraphQLApi.Models;
using GraphQLApi.Services;
using Microsoft.EntityFrameworkCore;

namespace GraphQLApi.GraphQL.Resolvers;

[ExtendObjectType<Mutation>]
public class AuthorMutationResolvers
{
    /// <summary>
    /// Add a new author
    /// </summary>
    public async Task<Author> AddAuthor([Service] BookService bookService, string name, string email, DateTime dateOfBirth)
    {
        return await bookService.AddAuthorAsync(name, email, dateOfBirth);
    }

    /// <summary>
    /// Update an existing author
    /// </summary>
    public async Task<Author?> UpdateAuthor([Service] BookService bookService, int id, string? name = null, string? email = null, DateTime? dateOfBirth = null)
    {
        return await bookService.UpdateAuthorAsync(id, name, email, dateOfBirth);
    }

    /// <summary>
    /// Delete an author
    /// </summary>
    public async Task<bool> DeleteAuthor([Service] BookService bookService, int id)
    {
        return await bookService.DeleteAuthorAsync(id);
    }

    /// <summary>
    /// Add author with validation
    /// </summary>
    public async Task<Author> AddAuthorWithValidation([Service] BookService bookService, string name, string email, DateTime dateOfBirth)
    {
        // Validate email format
        if (!email.Contains("@"))
        {
            throw new ArgumentException("Invalid email format");
        }

        // Validate date of birth
        if (dateOfBirth > DateTime.Now)
        {
            throw new ArgumentException("Date of birth cannot be in the future");
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        {
            throw new ArgumentException("Name must be at least 2 characters long");
        }

        return await bookService.AddAuthorAsync(name, email, dateOfBirth);
    }

    /// <summary>
    /// Update author with validation
    /// </summary>
    public async Task<Author?> UpdateAuthorWithValidation([Service] BookService bookService, int id, string? name = null, string? email = null, DateTime? dateOfBirth = null)
    {
        // Validate email format if provided
        if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
        {
            throw new ArgumentException("Invalid email format");
        }

        // Validate date of birth if provided
        if (dateOfBirth.HasValue && dateOfBirth.Value > DateTime.Now)
        {
            throw new ArgumentException("Date of birth cannot be in the future");
        }

        // Validate name if provided
        if (!string.IsNullOrEmpty(name) && name.Length < 2)
        {
            throw new ArgumentException("Name must be at least 2 characters long");
        }

        return await bookService.UpdateAuthorAsync(id, name, email, dateOfBirth);
    }

    /// <summary>
    /// Bulk delete authors without books
    /// </summary>
    public async Task<IEnumerable<Author>> BulkDeleteAuthorsWithoutBooks([Service] BookService bookService)
    {
        var authors = await bookService.GetAuthors().ToListAsync();
        var deletedAuthors = new List<Author>();

        foreach (var author in authors)
        {
            var booksCount = await bookService.GetBooksCountForAuthorAsync(author.Id);
            if (booksCount == 0)
            {
                var success = await bookService.DeleteAuthorAsync(author.Id);
                if (success)
                    deletedAuthors.Add(author);
            }
        }

        return deletedAuthors;
    }

    /// <summary>
    /// Transfer books from one author to another
    /// </summary>
    public async Task<bool> TransferBooksBetweenAuthors([Service] BookService bookService, int fromAuthorId, int toAuthorId)
    {
        // Verify both authors exist
        var fromAuthor = await bookService.GetAuthorByIdAsync(fromAuthorId);
        var toAuthor = await bookService.GetAuthorByIdAsync(toAuthorId);

        if (fromAuthor == null || toAuthor == null)
        {
            throw new ArgumentException("One or both authors not found");
        }

        // Get books from source author
        var books = await bookService.GetBooksByAuthorIdAsync(fromAuthorId);

        // Note: This would require a new method in BookService to update authorId
        // For now, we'll throw an exception indicating this feature needs implementation
        throw new NotImplementedException("Transferring books between authors requires additional BookService method implementation");
    }
}