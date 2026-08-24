namespace MasrLab.Domain.Common;

public static class CultureGroup
{
    public const string Name = "Culture and Sensitivity";

    public static bool IsCulture(string? group)
        => string.Equals(group, Name, StringComparison.OrdinalIgnoreCase);
}
