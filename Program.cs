using GraphQLApi.GraphQL;
using GraphQLApi.GraphQL.Resolvers;
using GraphQLApi.Services;
using GraphQLApi.Data;
using GraphQLApi.Repositories;
using GraphQLApi.ExternalServices;
using GraphQLApi.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure strongly-typed configuration
builder.Services.Configure<DatabaseConfiguration>(
    builder.Configuration.GetSection(DatabaseConfiguration.SectionName));
builder.Services.Configure<ExternalApiConfiguration>(
    builder.Configuration.GetSection(ExternalApiConfiguration.SectionName));

// Configure database contexts with new configuration structure
var dbConfig = builder.Configuration.GetSection(DatabaseConfiguration.SectionName).Get<DatabaseConfiguration>()
               ?? new DatabaseConfiguration();

builder.Services.AddDbContext<AuthorDbContext>(options =>
    options.UseSqlServer(dbConfig.AuthorDatabase));

builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlServer(dbConfig.BookDatabase));

// Register repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

// Register domain services (replacing the old BookService)
builder.Services.AddScoped<BookDomainService>();
builder.Services.AddScoped<AuthorDomainService>();

// Configure HttpClients for external APIs
var apiConfig = builder.Configuration.GetSection(ExternalApiConfiguration.SectionName).Get<ExternalApiConfiguration>()
                ?? new ExternalApiConfiguration();

builder.Services.AddHttpClient<IInventoryApiClient, InventoryApiClient>(client =>
{
    client.BaseAddress = new Uri(apiConfig.Inventory.BaseUrl);
    client.DefaultRequestHeaders.Add("X-API-Key", apiConfig.Inventory.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(apiConfig.Inventory.TimeoutSeconds);
});

// Add other external API clients as needed
// builder.Services.AddHttpClient<IRecommendationApiClient, RecommendationApiClient>(...)
// builder.Services.AddHttpClient<IPaymentApiClient, PaymentApiClient>(...)

// Add GraphQL services with resolvers
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<BookResolvers>()
    .AddTypeExtension<AuthorResolvers>()
    .AddTypeExtension<BookMutationResolvers>()
    .AddTypeExtension<AuthorMutationResolvers>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Create databases and apply migrations
using (var scope = app.Services.CreateScope())
{
    var authorContext = scope.ServiceProvider.GetRequiredService<AuthorDbContext>();
    var bookContext = scope.ServiceProvider.GetRequiredService<BookDbContext>();

    authorContext.Database.EnsureCreated();
    bookContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseHttpsRedirection();

// Map GraphQL endpoint
app.MapGraphQL();

// Optional: Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// Add database info endpoint
app.MapGet("/database-info", async (AuthorDbContext authorContext, BookDbContext bookContext) =>
{
    var authorCount = await authorContext.Authors.CountAsync();
    var bookCount = await bookContext.Books.CountAsync();

    return Results.Ok(new
    {
        AuthorDatabase = new { Count = authorCount, Database = "AuthorDb" },
        BookDatabase = new { Count = bookCount, Database = "BookDb" },
        Timestamp = DateTime.UtcNow
    });
});

app.Run();
