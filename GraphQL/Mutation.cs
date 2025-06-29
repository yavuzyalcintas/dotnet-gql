using GraphQLApi.Models;
using GraphQLApi.Services;

namespace GraphQLApi.GraphQL;

/// <summary>
/// Base mutation class. Most mutations are now handled by resolver classes:
/// - BookMutationResolvers for book-related mutations
/// - AuthorMutationResolvers for author-related mutations
/// </summary>
public class Mutation
{
    // This class can be empty or contain basic mutations
    // Most mutations are now handled by the resolver classes for better organization
}