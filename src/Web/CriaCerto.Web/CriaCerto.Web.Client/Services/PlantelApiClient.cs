using System.Net.Http.Headers;
using System.Net.Http.Json;
using CriaCerto.Web.Client.Models;
using Microsoft.JSInterop;

namespace CriaCerto.Web.Client.Services;

public sealed class PlantelApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public PlantelApiClient(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<PlantelListResponse<SowSummaryDto>?> ListSowsAsync(string? search, ReproductiveStatus? status, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search)}");
        }

        if (status.HasValue)
        {
            query.Add($"status={status.Value}");
        }

        var url = "api/breeding/sows" + (query.Count > 0 ? "?" + string.Join('&', query) : string.Empty);
        return await _httpClient.GetFromJsonAsync<PlantelListResponse<SowSummaryDto>>(url, cancellationToken);
    }

    public async Task<SowDetailDto?> GetSowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        return await _httpClient.GetFromJsonAsync<SowDetailDto>($"api/breeding/sows/{id}", cancellationToken);
    }

    private async Task AttachTokenAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }
}
