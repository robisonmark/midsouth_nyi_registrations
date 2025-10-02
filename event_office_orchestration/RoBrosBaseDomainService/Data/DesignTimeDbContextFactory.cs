using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RoBrosBaseDomainService.Data;

/// <summary>
/// Factory for creating DbContext at design time (for migrations)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<JournalDbContext>
{
    public JournalDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<JournalDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=robros_journal_db;Username=robros_user;Password=robros_password";

        optionsBuilder.UseNpgsql(connectionString);

        return new JournalDbContext(optionsBuilder.Options);
    }
}