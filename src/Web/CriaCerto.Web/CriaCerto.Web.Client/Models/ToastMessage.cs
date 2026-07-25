namespace CriaCerto.Web.Client.Models;

public record ToastMessage(
    Guid Id,
    string Title,
    string Message,
    ToastLevel Level,
    DateTime Timestamp,
    int DurationMs = 5000);
