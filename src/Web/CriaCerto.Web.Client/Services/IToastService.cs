using CriaCerto.Web.Client.Models;

namespace CriaCerto.Web.Client.Services;

public interface IToastService
{
    event Action<ToastMessage>? OnShow;
    event Action<Guid>? OnHide;

    IReadOnlyList<ToastMessage> ActiveToasts { get; }

    void ShowSuccess(string message, string? title = null, int durationMs = 5000);
    void ShowError(string message, string? title = null, int durationMs = 7000);
    void ShowWarning(string message, string? title = null, int durationMs = 6000);
    void ShowInfo(string message, string? title = null, int durationMs = 5000);
    void RemoveToast(Guid id);
}
