namespace NexusLocal.Services;

public sealed class AppEnvironment
{
    public string ExecutableDirectory { get; }
    public string? SolutionRoot { get; }
    public string RootDirectory { get; }
    public string DataDirectory { get; }
    public bool IsDevelopmentMode { get; }

    public AppEnvironment()
    {
        ExecutableDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
        SolutionRoot = FindSolutionRoot(ExecutableDirectory);
        IsDevelopmentMode = SolutionRoot is not null;

        RootDirectory = ResolveRootDirectory();
        DataDirectory = Path.Combine(RootDirectory, "data");
    }

    private string ResolveRootDirectory()
    {
        if (SolutionRoot is not null)
        {
            return SolutionRoot;
        }

        var currentDirectory = new DirectoryInfo(ExecutableDirectory);
        if (string.Equals(currentDirectory.Name, "current", StringComparison.OrdinalIgnoreCase) &&
            currentDirectory.Parent is not null)
        {
            return currentDirectory.Parent.FullName;
        }

        return ExecutableDirectory;
    }

    private static string? FindSolutionRoot(string startDirectory)
    {
        var currentDirectory = new DirectoryInfo(startDirectory);
        while (currentDirectory is not null)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, "BhmArAutoUpdater.slnx")))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}
