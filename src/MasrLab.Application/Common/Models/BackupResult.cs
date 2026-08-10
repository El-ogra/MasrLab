namespace MasrLab.Application.Common.Models;

public record BackupResult(string DatabaseName, string FilePath, DateTimeOffset CompletedAt);
