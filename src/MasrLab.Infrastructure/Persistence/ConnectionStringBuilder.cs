using Microsoft.Extensions.Configuration;

namespace MasrLab.Infrastructure.Persistence;

public static class ConnectionStringBuilder
{
    private const string TrustServerCertificateKey = "TrustServerCertificate";

    public static string GetProductionConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);

        if (builder.TrustServerCertificate)
        {
            builder.TrustServerCertificate = false;
            builder.Encrypt = true;
        }

        return builder.ConnectionString;
    }

    public static string GetDevelopmentConnectionString(IConfiguration configuration)
    {
        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }
}
