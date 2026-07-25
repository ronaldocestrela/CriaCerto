using System.Net.Http.Headers;
using System.Net.Http.Json;
using CriaCerto.Web.Client.Models;
using Microsoft.JSInterop;

namespace CriaCerto.Web.Client.Services;

public sealed class BreedingOpsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public BreedingOpsApiClient(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<IatfProtocolDto?> RegisterIatfProtocolAsync(string name, DateTime startDate, DateTime inseminationDate, Guid semenBatchId, List<Guid> cowIds, Guid tenantId, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var request = new { Name = name, StartDate = startDate, InseminationDate = inseminationDate, SemenBatchId = semenBatchId, CowIds = cowIds, TenantId = tenantId };
        var response = await _httpClient.PostAsJsonAsync("api/breeding/iatf-protocols", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<IatfProtocolDto>(cancellationToken: cancellationToken);
    }

    private async Task AttachTokenAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }
}
