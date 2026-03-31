namespace Launcher.Services;

public sealed class LaunchResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private LaunchResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static LaunchResult Succeeded() => new(true, null);

    public static LaunchResult Failed(string errorMessage) => new(false, errorMessage);
}
