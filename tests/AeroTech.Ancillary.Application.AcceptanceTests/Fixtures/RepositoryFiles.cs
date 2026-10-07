namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class RepositoryFiles
{
    private const string Solution = "AeroTech.Ancillary.sln";

    public static string Root { get; } = FindRoot();

    public static string TestProjectFile(string name)
        => Path.Combine(Root, "tests", typeof(RepositoryFiles).Assembly.GetName().Name!, name);

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, Solution)))
            directory = directory.Parent;

        return directory?.FullName
               ?? throw new InvalidOperationException($"The repository root with {Solution} was not found above {AppContext.BaseDirectory}.");
    }
}
