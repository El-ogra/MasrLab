using System.Text.RegularExpressions;

namespace MasrLab.Infrastructure.Tests;

public class AsyncCancellationPolicyTests
{
    private static readonly Regex[] ForbiddenCalls =
    [
        new(@"\.GetAllAsync\s*\(\s*\)", RegexOptions.Compiled),
        new(@"\.SaveChangesAsync\s*\(\s*\)", RegexOptions.Compiled)
    ];

    [Fact]
    public void SourceFiles_DoNotCallGetAllOrSaveChangesWithoutCancellationToken()
    {
        var repositoryRoot = FindRepositoryRoot();
        var violations = new List<string>();

        foreach (var directory in new[] { "src", "tests" })
        {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(repositoryRoot, directory), "*.cs", SearchOption.AllDirectories))
            {
                var lines = File.ReadAllLines(file);
                for (var index = 0; index < lines.Length; index++)
                {
                    if (ForbiddenCalls.Any(pattern => pattern.IsMatch(lines[index])))
                        violations.Add($"{Path.GetRelativePath(repositoryRoot, file)}:{index + 1}: {lines[index].Trim()}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "Calls without CancellationToken were found:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MasrLab.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root containing MasrLab.sln.");
    }
}
