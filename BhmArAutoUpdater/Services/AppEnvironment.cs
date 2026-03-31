using System.Text.RegularExpressions;

namespace BhmArAutoUpdater.Services;

public sealed class AppEnvironment
{
    private static readonly Regex FolderNamePattern = new(
        @"^BhmArAutoUpdater_(?<version>\d+\.\d+\.\d+)_win-x64$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string ExecutableDirectory { get; }
    public string? SolutionRoot { get; }
    public string AppRoot { get; }
    public bool IsDevelopmentMode { get; }
    public string? CurrentFolderName { get; }
    public Version? CurrentVersion { get; }

    public AppEnvironment()
    {
        ExecutableDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
        SolutionRoot = FindSolutionRoot(ExecutableDirectory);

        var currentFolderName = new DirectoryInfo(ExecutableDirectory).Name;
        CurrentFolderName = IsVersionedFolder(currentFolderName) ? currentFolderName : null;
        CurrentVersion = ParseVersion(currentFolderName);

        if (CurrentFolderName is not null)
        {
            AppRoot = Directory.GetParent(ExecutableDirectory)?.FullName ?? ExecutableDirectory;
            IsDevelopmentMode = false;
            return;
        }

        AppRoot = SolutionRoot is null
            ? Path.Combine(ExecutableDirectory, "app")
            : Path.Combine(SolutionRoot, "app");
        IsDevelopmentMode = SolutionRoot is not null;
    }

    public string GetVersionFolderName(Version version) => $"BhmArAutoUpdater_{version}_win-x64";

    public Version? ParseVersion(string pathOrFolderName)
    {
        var folderName = Path.GetFileName(pathOrFolderName);
        var match = FolderNamePattern.Match(folderName);
        if (!match.Success)
        {
            return null;
        }

        return Version.TryParse(match.Groups["version"].Value, out var version) ? version : null;
    }

    public bool IsVersionedFolder(string pathOrFolderName) => ParseVersion(pathOrFolderName) is not null;

    private static string? FindSolutionRoot(string startDirectory)
    {
        var currentDirectory = new DirectoryInfo(startDirectory);
        while (currentDirectory is not null)
        {
            var solutionPath = Path.Combine(currentDirectory.FullName, "BhmArAutoUpdater.slnx");
            if (File.Exists(solutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}
