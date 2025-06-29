using Microsoft.EntityFrameworkCore;
using GraphQLApi.Models;

namespace GraphQLApi.Data;

public class AuthorDbContext : DbContext
{
    public AuthorDbContext(DbContextOptions<AuthorDbContext> options) : base(options)
    {
    }

    public DbSet<Author> Authors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Author entity
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Add unique constraint on email
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Seed data
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, Name = "J.K. Rowling", Email = "jk@example.com", DateOfBirth = new DateTime(1965, 7, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Author { Id = 2, Name = "George R.R. Martin", Email = "george@example.com", DateOfBirth = new DateTime(1948, 9, 20), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Author { Id = 3, Name = "Stephen King", Email = "stephen@example.com", DateOfBirth = new DateTime(1947, 9, 21), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
    }
}