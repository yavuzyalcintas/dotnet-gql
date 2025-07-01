# Scalable GraphQL Architecture Guide

## 🏗️ Architecture Overview

This project implements a scalable GraphQL architecture designed to handle **5 databases** and **multiple external APIs** while maintaining your existing GraphQL resolver pattern.

## 📁 Project Structure

```
GraphQLApi/
├── Configuration/           # Strongly-typed configuration classes
├── Data/                   # Entity Framework DbContexts
├── ExternalServices/       # External API clients with resilience
├── GraphQL/               # GraphQL schema and resolvers
├── Models/                # Domain entities
├── Repositories/          # Repository pattern for data access
└── Services/              # Domain-specific business logic services
```

## 🎯 Key Architectural Decisions

### 1. **Repository Pattern**

- **Why**: Abstracts database access, easier testing, consistent data operations
- **Scale**: Each new database gets its own repository interface/implementation
- **Example**: `IBookRepository`, `IAuthorRepository`

### 2. **Domain Services**

- **Why**: Replaces monolithic service with focused domain logic
- **Scale**: One service per domain (Books, Authors, Users, Orders, etc.)
- **Example**: `BookDomainService`, `AuthorDomainService`

### 3. **External API Integration**

- **Why**: Consistent patterns, resilience, logging, timeout handling
- **Scale**: Interface-based clients with HttpClient integration
- **Example**: `IInventoryApiClient`, `IPaymentApiClient`

### 4. **Configuration Management**

- **Why**: Centralized config for multiple databases and APIs
- **Scale**: Strongly-typed configuration sections
- **Example**: `DatabaseConfiguration`, `ExternalApiConfiguration`

## 🗄️ Adding New Databases

### Step 1: Create the DbContext

```csharp
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    // Configure entities in OnModelCreating
}
```

### Step 2: Add Connection String

```json
"DatabaseConnections": {
  "UserDatabase": "Server=...;Database=UserDb;..."
}
```

### Step 3: Register in Program.cs

```csharp
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(dbConfig.UserDatabase));
```

### Step 4: Create Repository

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository : IUserRepository
{
    // Implement interface
}
```

### Step 5: Register Repository

```csharp
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

### Step 6: Create Domain Service

```csharp
public class UserDomainService
{
    private readonly IUserRepository _userRepository;
    // Implement domain logic
}
```

## 🌐 Adding External APIs

### Step 1: Define Interface and DTOs

```csharp
public interface IUserProfileApiClient
{
    Task<UserProfile?> GetProfileAsync(int userId);
}

public class UserProfile
{
    public int UserId { get; set; }
    public string DisplayName { get; set; }
    // Other properties
}
```

### Step 2: Implement Client

```csharp
public class UserProfileApiClient : IUserProfileApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserProfileApiClient> _logger;

    // Implement with error handling, logging, timeouts
}
```

### Step 3: Add Configuration

```json
"ExternalApis": {
  "UserProfile": {
    "BaseUrl": "https://api.userprofile.com",
    "ApiKey": "your-api-key",
    "TimeoutSeconds": 30,
    "RetryCount": 3
  }
}
```

### Step 4: Register HttpClient

```csharp
builder.Services.AddHttpClient<IUserProfileApiClient, UserProfileApiClient>(client =>
{
    client.BaseAddress = new Uri(apiConfig.UserProfile.BaseUrl);
    client.DefaultRequestHeaders.Add("X-API-Key", apiConfig.UserProfile.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(apiConfig.UserProfile.TimeoutSeconds);
});
```

## 🔧 GraphQL Integration

### Update Resolvers

```csharp
[ExtendObjectType<Query>]
public class UserResolvers
{
    public async Task<IEnumerable<User>> GetUsers([Service] UserDomainService userService)
        => await userService.GetUsersAsync();

    public async Task<UserProfile?> GetUserProfile([Service] UserDomainService userService, int userId)
        => await userService.GetUserProfileAsync(userId);
}
```

### Register Resolvers

```csharp
builder.Services
    .AddGraphQLServer()
    .AddTypeExtension<UserResolvers>()
    // ... other configurations
```

## 📊 Benefits of This Architecture

### ✅ **Maintainability**

- Clear separation of concerns
- Each database/API has focused responsibilities
- Easy to test individual components

### ✅ **Scalability**

- Add new databases without touching existing code
- External API failures don't affect others
- Independent scaling of read/write operations

### ✅ **Resilience**

- Built-in error handling and logging
- Timeout and retry mechanisms
- Graceful degradation when external services fail

### ✅ **Developer Experience**

- Strongly-typed configuration
- Consistent patterns across all data sources
- GraphQL schema remains clean and intuitive

## 🚀 Migration Strategy

### Phase 1: Foundation (✅ Complete)

- Repository pattern
- Domain services
- External API integration
- Configuration management

### Phase 2: Add Databases

```
1. InventoryDbContext + Repository + Service
2. UserDbContext + Repository + Service
3. OrderDbContext + Repository + Service
4. Analytics/ReportingDbContext + Repository + Service
```

### Phase 3: Add External APIs

```
1. RecommendationApiClient
2. PaymentApiClient
3. NotificationApiClient
4. AnalyticsApiClient
```

### Phase 4: Advanced Features

```
1. Caching layer (Redis)
2. Background jobs (Hangfire)
3. Event-driven architecture
4. API rate limiting
```

## 🛡️ Best Practices

### Database Access

- Always use repositories, never direct DbContext in resolvers
- Implement proper error handling in repositories
- Use transactions for multi-repository operations

### External API Calls

- Always include timeout and retry logic
- Log all external API calls for monitoring
- Implement circuit breaker pattern for critical APIs
- Cache responses when appropriate

### Configuration

- Use strongly-typed configuration classes
- Validate configuration on startup
- Support environment-specific settings

### GraphQL Resolvers

- Keep resolvers thin - delegate to domain services
- Use proper GraphQL attributes for filtering/sorting
- Handle null cases gracefully

## 🔍 Monitoring and Logging

### Built-in Logging

- All repositories log CRUD operations
- External API clients log requests/responses/errors
- Domain services log business operations

### Health Checks

```csharp
app.MapGet("/health", () => Results.Ok(new {
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Databases = /* check each database */,
    ExternalApis = /* check each API */
}));
```

This architecture provides a solid foundation that can grow from 2 databases to 5+ databases and multiple external APIs while maintaining code quality and developer productivity.
