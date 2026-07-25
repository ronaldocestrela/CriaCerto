using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CriaCerto.Web.Client.Models;
using Microsoft.JSInterop;

namespace CriaCerto.Web.Client.Services;

public sealed class MaternityApiClient
{
    private const string PendingQueueKey = "maternity_pending_farrowings";
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public MaternityApiClient(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<List<FarrowingSummaryClientDto>> GetFarrowingsAsync(
        Guid? sowId = null,
        string? maternityRoomId = null,
        CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync();
        var queryParams = new List<string>();

        if (sowId.HasValue && sowId.Value != Guid.Empty)
        {
            queryParams.Add($"sowId={sowId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(maternityRoomId))
        {
            queryParams.Add($"maternityRoomId={Uri.EscapeDataString(maternityRoomId)}");
        }

        var url = "api/maternity/farrowings";
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<FarrowingSummaryClientDto>>(url, cancellationToken);
            return result ?? new List<FarrowingSummaryClientDto>();
        }
        catch
        {
            return new List<FarrowingSummaryClientDto>();
        }
    }

    public async Task<FarrowingClientDto?> RegisterFarrowingAsync(
        RegisterFarrowingRequest request,
        CancellationToken cancellationToken = default)
    {
        bool online = await IsOnlineAsync();
        if (!online)
        {
            await QueueOfflineRegistrationAsync(request);
            return null;
        }

        try
        {
            await AttachTokenAsync();
            var response = await _httpClient.PostAsJsonAsync("api/maternity/farrowings", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await QueueOfflineRegistrationAsync(request);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FarrowingClientDto>(cancellationToken: cancellationToken);
        }
        catch
        {
            await QueueOfflineRegistrationAsync(request);
            return null;
        }
    }

    public async Task<int> GetPendingOfflineCountAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", PendingQueueKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return 0;
            }

            var queue = JsonSerializer.Deserialize<List<RegisterFarrowingRequest>>(json);
            return queue?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> FlushOfflineQueueAsync(CancellationToken cancellationToken = default)
    {
        bool online = await IsOnlineAsync();
        if (!online)
        {
            return 0;
        }

        var count = await GetPendingOfflineCountAsync();
        if (count == 0)
        {
            return 0;
        }

        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", PendingQueueKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return 0;
            }

            var queue = JsonSerializer.Deserialize<List<RegisterFarrowingRequest>>(json) ?? new();
            var remaining = new List<RegisterFarrowingRequest>();
            int synced = 0;

            await AttachTokenAsync();
            foreach (var item in queue)
            {
                var response = await _httpClient.PostAsJsonAsync("api/maternity/farrowings", item, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    synced++;
                }
                else
                {
                    remaining.Add(item);
                }
            }

            if (remaining.Count > 0)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", PendingQueueKey, JsonSerializer.Serialize(remaining));
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", PendingQueueKey);
            }

            return synced;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("eval", "navigator.onLine");
        }
        catch
        {
            return true;
        }
    }

    private async Task QueueOfflineRegistrationAsync(RegisterFarrowingRequest request)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", PendingQueueKey);
            var queue = string.IsNullOrWhiteSpace(json)
                ? new List<RegisterFarrowingRequest>()
                : JsonSerializer.Deserialize<List<RegisterFarrowingRequest>>(json) ?? new();

            queue.Add(request);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", PendingQueueKey, JsonSerializer.Serialize(queue));
        }
        catch
        {
            // Fallback ignore if JS interop fails
        }
    }

    private async Task AttachTokenAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Ignored when running pre-render or during JS disconnect
        }
    }
}
