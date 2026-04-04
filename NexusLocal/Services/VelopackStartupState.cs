namespace NexusLocal.Services;

public sealed class VelopackStartupState
{
    public VelopackStartupState()
    {
        UpdatedFromVersion = ReadArg("--updated-from");
        UpdatedToVersion = ReadArg("--updated-to");
        UpdatedPackage = ReadArg("--updated-package");
    }

    public string? FirstRunVersion { get; set; }
    public string? RestartedVersion { get; set; }
    public string? UpdatedFromVersion { get; }
    public string? UpdatedToVersion { get; }
    public string? UpdatedPackage { get; }

    private static string? ReadArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++) {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) {
                return args[i + 1];
            }
        }

        return null;
    }
}
