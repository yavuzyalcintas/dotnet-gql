using Microsoft.EntityFrameworkCore;
using GraphQLApi.Models;

namespace GraphQLApi.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Book entity
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.AuthorId).IsRequired();
            entity.Property(e => e.PublishedDate).IsRequired();
            entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsAvailable).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Add index on AuthorId for better performance
            entity.HasIndex(e => e.AuthorId);
        });

        // Seed data
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Harry Potter and the Philosopher's Stone", Description = "A young wizard's journey begins", AuthorId = 1, PublishedDate = new DateTime(1997, 6, 26), Price = 19.99m, IsAvailable = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Book { Id = 2, Title = "Harry Potter and the Chamber of Secrets", Description = "The second year at Hogwarts", AuthorId = 1, PublishedDate = new DateTime(1998, 7, 2), Price = 19.99m, IsAvailable = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Book { Id = 3, Title = "A Game of Thrones", Description = "The first book in A Song of Ice and Fire series", AuthorId = 2, PublishedDate = new DateTime(1996, 8, 1), Price = 24.99m, IsAvailable = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Book { Id = 4, Title = "The Shining", Description = "A horror novel about isolation and madness", AuthorId = 3, PublishedDate = new DateTime(1977, 1, 28), Price = 22.99m, IsAvailable = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
    }
}