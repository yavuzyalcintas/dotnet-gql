namespace GraphQLApi.ExternalServices;

public interface IExternalApiClient<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(int id, T entity);
    Task<bool> DeleteAsync(int id);
}

public interface IExternalApiClient<T, TCreateRequest, TUpdateRequest>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(TCreateRequest request);
    Task<T?> UpdateAsync(int id, TUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}

// Specific external service interfaces
public interface IInventoryApiClient
{
    Task<InventoryItem?> GetInventoryByBookIdAsync(int bookId);
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync();
    Task<bool> UpdateStockAsync(int bookId, int quantity);
}

public interface IRecommendationApiClient
{
    Task<IEnumerable<BookRecommendation>> GetRecommendationsForUserAsync(int userId);
    Task<IEnumerable<BookRecommendation>> GetSimilarBooksAsync(int bookId);
}

public interface IPaymentApiClient
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<PaymentStatus> GetPaymentStatusAsync(string transactionId);
}

// DTOs for external services
public class InventoryItem
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class BookRecommendation
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public int UserId { get; set; }
    public List<int> BookIds { get; set; } = new();
}

public class PaymentResult
{
    public string TransactionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentStatus
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}