namespace BhmArAutoUpdater.Services;

public sealed class ReleaseInstallResult
{
    public bool Success { get; }
    public string Message { get; }

    private ReleaseInstallResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public static ReleaseInstallResult Succeeded(string message) => new(true, message);

    public static ReleaseInstallResult Failed(string message) => new(false, message);
}
