# GraphQL API - Book Management System (Dual Database Architecture)

A .NET 8 GraphQL API built with HotChocolate that provides a book and author management system using **two separate databases** - one for authors and one for books. The API uses **GraphQL resolvers** to automatically handle cross-database relationships.

## Features

- **Dual Database Architecture**: Separate SQL Server databases for Authors and Books
- **GraphQL Resolvers**: Automatic cross-database relationship handling
- **GraphQL Queries**: Retrieve books, authors, and perform cross-database queries
- **GraphQL Mutations**: Create, update, and delete books and authors
- **Cross-Database Relationships**: Books reference authors through AuthorId
- **Filtering, Sorting, and Projections**: Advanced query capabilities via HotChocolate.Data
- **Entity Framework Core**: Full database integration with automatic database creation

## Database Architecture

### Author Database (`AuthorDb`)

- **Table**: Authors
- **Fields**: Id, Name, Email, DateOfBirth, CreatedAt, UpdatedAt
- **Connection String**: `AuthorDatabase` in appsettings.json

### Book Database (`BookDb`)

- **Table**: Books
- **Fields**: Id, Title, Description, AuthorId, PublishedDate, Price, IsAvailable, CreatedAt, UpdatedAt
- **Connection String**: `BookDatabase` in appsettings.json
- **Foreign Key**: AuthorId references Author.Id in the Author database

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (automatically installed with Visual Studio)
- Any HTTP client that supports GraphQL (Postman, Insomnia, etc.)

### Running the Application

```bash
dotnet run
```

The application will:

1. Create both databases automatically (`AuthorDb` and `BookDb`)
2. Seed initial data (3 authors and 4 books)
3. Start the GraphQL server

### GraphQL Endpoints

- **GraphQL Endpoint**: `/graphql`
- **GraphQL Playground**: Navigate to `/graphql` in your browser
- **Health Check**: `/health`
- **Database Info**: `/database-info` (shows record counts in both databases)

## GraphQL Schema

### Core Types

```graphql
type Book {
  id: Int!
  title: String!
  description: String!
  authorId: Int! # References Author in separate database
  publishedDate: DateTime!
  price: Decimal!
  isAvailable: Boolean!
  createdAt: DateTime!
  updatedAt: DateTime!

  # Resolver fields (automatic cross-database lookups)
  author: Author # Automatically fetches from AuthorDb
  authorName: String # Convenience field
  hasValidAuthor: Boolean # Validation field
  formattedPrice: String # Formatted with currency
  ageInYears: Int # Calculated age of book
}

type Author {
  id: Int!
  name: String!
  email: String!
  dateOfBirth: DateTime!
  createdAt: DateTime!
  updatedAt: DateTime!

  # Resolver fields (automatic cross-database lookups)
  books: [Book!]! # Automatically fetches from BookDb
  booksCount: Int! # Count of books by this author
  availableBooks: [Book!]! # Only available books
  totalBooksValue: Decimal! # Sum of all book prices
  mostExpensiveBook: Book # Most expensive book by this author
  age: Int # Current age of author
  yearsSinceFirstPublication: Int # Years since first book
}
```

### Queries

```graphql
type Query {
  # Basic queries
  books: [Book!]!
  authors: [Author!]!
  book(id: Int!): Book
  author(id: Int!): Author

  # Filtered queries
  availableBooks: [Book!]!
  searchBooks(searchTerm: String!): [Book!]!
}
```

### Mutations

```graphql
type Mutation {
  # Book mutations
  addBook(
    title: String!
    description: String!
    authorId: Int!
    price: Decimal!
  ): Book!
  updateBook(
    id: Int!
    title: String
    description: String
    price: Decimal
    isAvailable: Boolean
  ): Book
  deleteBook(id: Int!): Boolean!
  toggleBookAvailability(id: Int!): Book

  # Author mutations
  addAuthor(name: String!, email: String!, dateOfBirth: DateTime!): Author!
  updateAuthor(
    id: Int!
    name: String
    email: String
    dateOfBirth: DateTime
  ): Author
  deleteAuthor(id: Int!): Boolean!
}
```

## Resolver-Based Queries

The power of this API lies in its resolver-based approach. You can write simple, nested queries that automatically handle cross-database relationships:

### Simple Cross-Database Queries

```graphql
# Get books with author information (automatic cross-database lookup)
query {
  books {
    id
    title
    price
    author {
      name
      email
    }
  }
}

# Get authors with their books (automatic cross-database lookup)
query {
  authors {
    id
    name
    email
    books {
      title
      price
      isAvailable
    }
    booksCount
  }
}
```

### Advanced Resolver Queries

```graphql
# Get books with calculated fields
query {
  books {
    id
    title
    formattedPrice
    ageInYears
    hasValidAuthor
    authorName
  }
}

# Get authors with advanced calculated fields
query {
  authors {
    id
    name
    age
    yearsSinceFirstPublication
    totalBooksValue
    mostExpensiveBook {
      title
      price
    }
    availableBooks {
      title
      price
    }
  }
}
```

### Filtered Queries with Resolvers

```graphql
# Get expensive books with author details
query {
  books(where: { price: { gt: 20.0 } }) {
    id
    title
    formattedPrice
    author {
      name
      email
    }
  }
}

# Get authors sorted by name with book counts
query {
  authors(order: { name: ASC }) {
    id
    name
    booksCount
    totalBooksValue
  }
}
```

## Technology Stack

- **.NET 8**: Latest .NET framework
- **Entity Framework Core**: Database ORM with SQL Server
- **SQL Server LocalDB**: Development database
- **HotChocolate**: GraphQL server implementation
- **HotChocolate.AspNetCore**: ASP.NET Core integration
- **HotChocolate.Data**: Filtering, sorting, and projections
- **GraphQL Resolvers**: Automatic cross-database relationship handling

## Database Management

### Connection Strings

```json
{
  "ConnectionStrings": {
    "AuthorDatabase": "Server=(localdb)\\mssqllocaldb;Database=AuthorDb;Trusted_Connection=true;TrustServerCertificate=true",
    "BookDatabase": "Server=(localdb)\\mssqllocaldb;Database=BookDb;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

### Sample Data

The application automatically seeds:

- **Authors**: J.K. Rowling, George R.R. Martin, Stephen King
- **Books**: Harry Potter series, Game of Thrones, The Shining

## Architecture Benefits

1. **Maintainable**: Resolvers handle cross-database complexity automatically
2. **Intuitive**: Write simple nested queries that work across databases
3. **Scalable**: Each database can be scaled independently
4. **Flexible**: Easy to add new calculated fields via resolvers
5. **Performance**: Efficient queries with automatic data loading
6. **Clean Code**: No complex composite types or manual joins

## Resolver Implementation

The API uses two main resolver classes:

- **BookResolvers**: Adds fields like `author`, `authorName`, `formattedPrice`, `ageInYears`
- **AuthorResolvers**: Adds fields like `books`, `booksCount`, `totalBooksValue`, `age`

These resolvers automatically handle:

- Cross-database lookups
- Data validation
- Calculated fields
- Performance optimization

## Query Examples

For comprehensive query examples, see `simplified-resolver-queries.graphql` in the project root.
