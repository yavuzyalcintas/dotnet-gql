namespace GraphQLApi.Configuration;

public class DatabaseConfiguration
{
    public const string SectionName = "DatabaseConnections";

    public string AuthorDatabase { get; set; } = string.Empty;
    public string BookDatabase { get; set; } = string.Empty;
    public string InventoryDatabase { get; set; } = string.Empty;
    public string UserDatabase { get; set; } = string.Empty;
    public string OrderDatabase { get; set; } = string.Empty;
}

public class ExternalApiConfiguration
{
    public const string SectionName = "ExternalApis";

    public ApiEndpoint Inventory { get; set; } = new();
    public ApiEndpoint Recommendations { get; set; } = new();
    public ApiEndpoint Payment { get; set; } = new();
}

public class ApiEndpoint
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}