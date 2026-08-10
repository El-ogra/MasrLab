using MasrLab.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MasrLab.Infrastructure.Tests;

public class BackupServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"MasrLabBackupTests-{Guid.NewGuid():N}");

    public BackupServiceTests()
    {
        Directory.CreateDirectory(_directory);
    }

    [Fact]
    public async Task RestoreAsync_VerifyFailure_DoesNotSetSingleUser()
    {
        var executor = CreateExecutor();
        executor.Setup(x => x.ExecuteAsync(It.Is<string>(sql => sql.StartsWith("RESTORE VERIFYONLY")), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("invalid backup"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(executor).RestoreAsync(CreateBackupFile(), "RESTORE"));

        VerifyNoSingleUser(executor);
    }

    [Fact]
    public async Task RestoreAsync_HeaderFailure_DoesNotSetSingleUser()
    {
        var executor = CreateExecutor();
        executor.Setup(x => x.ReadHeadersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("header"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(executor).RestoreAsync(CreateBackupFile(), "RESTORE"));

        VerifyNoSingleUser(executor);
    }

    [Fact]
    public async Task RestoreAsync_FileListFailure_DoesNotSetSingleUser()
    {
        var executor = CreateExecutor();
        executor.Setup(x => x.ReadFileListAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("file list"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(executor).RestoreAsync(CreateBackupFile(), "RESTORE"));

        VerifyNoSingleUser(executor);
    }

    [Fact]
    public async Task RestoreAsync_IncompatibleBackup_DoesNotSetSingleUser()
    {
        var executor = CreateExecutor(header: new SqlBackupHeader("OtherDatabase", Guid.NewGuid(), 1, 16));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(executor).RestoreAsync(CreateBackupFile(), "RESTORE"));

        VerifyNoSingleUser(executor);
    }

    [Fact]
    public async Task RestoreAsync_RestoreFailure_AttemptsMultiUserInFinally()
    {
        var steps = new List<string>();
        var executor = CreateExecutor(steps);
        executor.Setup(x => x.ExecuteAsync(It.Is<string>(sql => sql.StartsWith("RESTORE DATABASE")), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .Callback((string _, IReadOnlyDictionary<string, object?> _, CancellationToken _) => steps.Add("restore"))
            .ThrowsAsync(new InvalidOperationException("restore"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(executor).RestoreAsync(CreateBackupFile(), "RESTORE"));

        Assert.Equal(["verify", "header", "file-list", "database-info", "single-user", "restore", "multi-user"], steps);
    }

    [Fact]
    public async Task RestoreAsync_Success_UsesRequiredOperationOrderAndRestoresMultiUser()
    {
        var steps = new List<string>();
        var executor = CreateExecutor(steps);

        await CreateService(executor).RestoreAsync(CreateBackupFile(), "RESTORE");

        Assert.Equal(["verify", "header", "file-list", "database-info", "single-user", "restore", "multi-user"], steps);
        executor.Verify(x => x.ExecuteAsync(It.Is<string>(sql => sql.StartsWith("RESTORE DATABASE") && !sql.Contains("WITH REPLACE")), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_WrongConfirmation_DoesNotExecuteSql()
    {
        var executor = CreateExecutor();

        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(executor).RestoreAsync(Path.Combine(_directory, "missing.bak"), "restore"));

        VerifyNoSql(executor);
    }

    [Fact]
    public async Task RestoreAsync_DisallowedPath_DoesNotExecuteSql()
    {
        var executor = CreateExecutor();

        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(executor).RestoreAsync(Path.Combine(Path.GetTempPath(), "outside.bak"), "RESTORE"));

        VerifyNoSql(executor);
    }

    private BackupService CreateService(Mock<ISqlServerBackupExecutor> executor) =>
        new(executor.Object, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=MasrLabDb;Trusted_Connection=True;",
            ["BackupSettings:Directory"] = _directory
        }).Build());

    private static Mock<ISqlServerBackupExecutor> CreateExecutor(List<string>? steps = null, SqlBackupHeader? header = null)
    {
        var familyGuid = Guid.Parse("72E9B7D7-1B95-4C58-8703-9CC948667F66");
        var executor = new Mock<ISqlServerBackupExecutor>();
        executor.Setup(x => x.ExecuteAsync(It.Is<string>(sql => sql.StartsWith("RESTORE VERIFYONLY")), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .Callback((string _, IReadOnlyDictionary<string, object?> _, CancellationToken _) => steps?.Add("verify"))
            .Returns(Task.CompletedTask);
        executor.Setup(x => x.ReadHeadersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => steps?.Add("header"))
            .ReturnsAsync([header ?? new SqlBackupHeader("MasrLabDb", familyGuid, 1, 16)]);
        executor.Setup(x => x.ReadFileListAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => steps?.Add("file-list"))
            .ReturnsAsync([
                new SqlBackupFile("MasrLabDb", "C:\\data\\MasrLabDb.mdf", "D", 1, Guid.NewGuid()),
                new SqlBackupFile("MasrLabDb_log", "C:\\data\\MasrLabDb_log.ldf", "L", 2, Guid.NewGuid())
            ]);
        executor.Setup(x => x.GetDatabaseInfoAsync("MasrLabDb", It.IsAny<CancellationToken>()))
            .Callback(() => steps?.Add("database-info"))
            .ReturnsAsync(new SqlDatabaseInfo("MasrLabDb", familyGuid, "MULTI_USER", "ONLINE", 16));
        executor.Setup(x => x.ExecuteAsync(It.Is<string>(sql => sql.Contains("SET SINGLE_USER")), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .Callback((string _, IReadOnlyDictionary<string, object?> _, CancellationToken _) => steps?.Add("single-user"))
            .Returns(Task.CompletedTask);
        executor.Setup(x => x.ExecuteAsync(It.Is<string>(sql => sql.StartsWith("RESTORE DATABASE")), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .Callback((string _, IReadOnlyDictionary<string, object?> _, CancellationToken _) => steps?.Add("restore"))
            .Returns(Task.CompletedTask);
        executor.Setup(x => x.ExecuteAsync(It.Is<string>(sql => sql.Contains("SET MULTI_USER")), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .Callback((string _, IReadOnlyDictionary<string, object?> _, CancellationToken _) => steps?.Add("multi-user"))
            .Returns(Task.CompletedTask);
        return executor;
    }

    private string CreateBackupFile()
    {
        var path = Path.Combine(_directory, $"{Guid.NewGuid():N}.bak");
        File.WriteAllText(path, "unit-test backup placeholder");
        return path;
    }

    private static void VerifyNoSingleUser(Mock<ISqlServerBackupExecutor> executor) =>
        executor.Verify(x => x.ExecuteAsync(It.Is<string>(sql => sql.Contains("SET SINGLE_USER")), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Never);

    private static void VerifyNoSql(Mock<ISqlServerBackupExecutor> executor)
    {
        executor.Verify(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Never);
        executor.Verify(x => x.ReadHeadersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        executor.Verify(x => x.ReadFileListAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        executor.Verify(x => x.GetDatabaseInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
