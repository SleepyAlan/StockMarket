using System.Net.Http.Json;

namespace StockMarket.Infrastructure.StockMarketData.Alpaca;

public sealed class AlpacaClient
{
    private readonly HttpClient _httpClient;

    public AlpacaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    //TODO: Evaluate if we want to use a common interface for all clients, or keep them separate.
    //      Unknown requirements for future third party api clients, so keep seperate for now, conjoin later if needed.
    public async Task<T?> GetAsync<T>(string relativePath, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient
            .GetAsync(relativePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }
}
