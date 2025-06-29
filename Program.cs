using GraphQLApi.GraphQL;
using GraphQLApi.GraphQL.Resolvers;
using GraphQLApi.Services;
using GraphQLApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<BookService>();

// Configure database contexts with SQL Server LocalDB
builder.Services.AddDbContext<AuthorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthorDatabase") ??
        "Server=(localdb)\\mssqllocaldb;Database=AuthorDb;Trusted_Connection=true;TrustServerCertificate=true"));

builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookDatabase") ??
        "Server=(localdb)\\mssqllocaldb;Database=BookDb;Trusted_Connection=true;TrustServerCertificate=true"));

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
