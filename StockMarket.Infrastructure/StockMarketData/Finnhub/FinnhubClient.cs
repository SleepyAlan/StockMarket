using System.Net.Http.Json;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub;

public sealed class FinnhubClient
{
    private readonly HttpClient _httpClient;

    public FinnhubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetAsync<T>(string relativePath, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient
            .GetAsync(relativePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }
}
