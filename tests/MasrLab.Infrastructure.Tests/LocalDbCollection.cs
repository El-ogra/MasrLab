using Xunit;

namespace MasrLab.Infrastructure.Tests;

/// <summary>
/// xUnit collection definition that serializes all LocalDB integration tests.
/// Running multiple database-creation tests concurrently overwhelms the LocalDB
/// instance and causes Connection Timeout Expired errors. This collection ensures
/// they execute one at a time while leaving non-LocalDB tests unaffected.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class LocalDbCollectionDefinition
{
    public const string Name = "LocalDb";
}
