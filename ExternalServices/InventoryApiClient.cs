using System.Text.Json;

namespace GraphQLApi.ExternalServices;

public class InventoryApiClient : IInventoryApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public InventoryApiClient(HttpClient httpClient, ILogger<InventoryApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<InventoryItem?> GetInventoryByBookIdAsync(int bookId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"inventory/books/{bookId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get inventory for book {BookId}. Status: {StatusCode}",
                    bookId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<InventoryItem>(content, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting inventory for book {BookId}", bookId);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout getting inventory for book {BookId}", bookId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting inventory for book {BookId}", bookId);
            return null;
        }
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("inventory/low-stock");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get low stock items. Status: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<InventoryItem>();
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<InventoryItem>>(content, _jsonOptions)
                   ?? Enumerable.Empty<InventoryItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock items");
            return Enumerable.Empty<InventoryItem>();
        }
    }

    public async Task<bool> UpdateStockAsync(int bookId, int quantity)
    {
        try
        {
            var request = new { BookId = bookId, Quantity = quantity };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"inventory/books/{bookId}", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated stock for book {BookId} to {Quantity}",
                    bookId, quantity);
                return true;
            }

            _logger.LogWarning("Failed to update stock for book {BookId}. Status: {StatusCode}",
                bookId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for book {BookId}", bookId);
            return false;
        }
    }
}