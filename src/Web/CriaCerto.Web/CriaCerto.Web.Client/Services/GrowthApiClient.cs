using System.Net.Http.Headers;
using System.Net.Http.Json;
using CriaCerto.Web.Client.Models;
using Microsoft.JSInterop;

namespace CriaCerto.Web.Client.Services;

public sealed class GrowthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public GrowthApiClient(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<List<PaddockStockingRateDto>> GetPaddocksAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var response = await _httpClient.GetFromJsonAsync<List<PaddockStockingRateDto>>($"api/growth/paddocks?tenantId={tenantId}", cancellationToken);
        return response ?? new List<PaddockStockingRateDto>();
    }

    public async Task<PaddockDto?> CreatePaddockAsync(CreatePaddockCommand command, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var response = await _httpClient.PostAsJsonAsync("api/growth/paddocks", command, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PaddockDto>(cancellationToken);
        }
        return null;
    }

    public async Task<List<LotDto>> GetLotsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var response = await _httpClient.GetFromJsonAsync<List<LotDto>>($"api/growth/lots?tenantId={tenantId}", cancellationToken);
        return response ?? new List<LotDto>();
    }

    public async Task<LotDto?> CreateLotAsync(CreateLotCommand command, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var response = await _httpClient.PostAsJsonAsync("api/growth/lots", command, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LotDto>(cancellationToken);
        }
        return null;
    }

    public async Task<LotMovementDto?> MoveLotAsync(MoveLotToPaddockCommand command, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var response = await _httpClient.PostAsJsonAsync("api/growth/lots/move", command, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LotMovementDto>(cancellationToken);
        }
        return null;
    }

    public async Task<LotDto?> CloseLotAsync(Guid lotId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var response = await _httpClient.PostAsync($"api/growth/lots/{lotId}/close?tenantId={tenantId}", null, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LotDto>(cancellationToken);
        }
        return null;
    }

    private async Task AttachTokenAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }
}
