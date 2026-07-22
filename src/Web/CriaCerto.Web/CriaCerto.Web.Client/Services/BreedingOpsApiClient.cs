using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<PregnancyCheckQueueResponse?> ListPregnancyChecksAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var url = "api/breeding/pregnancy-checks";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search)}";
        }

        return await _httpClient.GetFromJsonAsync<PregnancyCheckQueueResponse>(url, cancellationToken);
    }

    public async Task<RegisterBreedingBatchResponse?> RegisterBreedingBatchAsync(RegisterBreedingBatchRequest request, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var response = await _httpClient.PostAsJsonAsync("api/breeding/events/batch", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<RegisterBreedingBatchResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PregnancyDiagnosisDto?> RegisterDiagnosisAsync(RegisterPregnancyDiagnosisRequest request, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var response = await _httpClient.PostAsJsonAsync("api/breeding/diagnoses", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PregnancyDiagnosisDto>(cancellationToken: cancellationToken);
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("plantelCache.isOnline");
        }
        catch
        {
            return true;
        }
    }

    public async Task<int> GetPendingOpsCountAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<int>("plantelCache.getPendingOpsCount");
        }
        catch
        {
            return 0;
        }
    }

    public async Task EnqueueBreedingBatchAsync(RegisterBreedingBatchRequest request)
    {
        var payload = JsonSerializer.Serialize(request);
        await _jsRuntime.InvokeVoidAsync("plantelCache.enqueueOp", "breedingBatch", payload);
    }

    public async Task EnqueueDiagnosisAsync(RegisterPregnancyDiagnosisRequest request)
    {
        var payload = JsonSerializer.Serialize(request);
        await _jsRuntime.InvokeVoidAsync("plantelCache.enqueueOp", "diagnosis", payload);
    }

    public async Task<int> SyncPendingOpsAsync(Func<string, string, Task<bool>> syncHandler)
    {
        var pending = await _jsRuntime.InvokeAsync<List<PendingOpDto>>("plantelCache.getPendingOps");
        var synced = 0;
        foreach (var op in pending)
        {
            var ok = await syncHandler(op.Type, op.Payload);
            if (ok)
            {
                await _jsRuntime.InvokeVoidAsync("plantelCache.removeOp", op.Id);
                synced++;
            }
        }

        return synced;
    }

    private async Task AttachTokenAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public sealed record PendingOpDto(string Id, string Type, string Payload, string CreatedAt);
}
