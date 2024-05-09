using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace EntityConfiguration;

[PrimaryKey(nameof(Key))]
public record class EntityConfigurationValue(string Key, string? Value);

public sealed class EntityConfigurationContext(string? connectionString) : DbContext
{
    public DbSet<EntityConfigurationValue> EntityConfigurationValues => Set<EntityConfigurationValue>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = connectionString switch
        {
            { Length: > 0 } => optionsBuilder.UseSqlite(connectionString),
            _ => optionsBuilder.UseSqlite("app,default.db")
        };
    }
}

public sealed class EntityConfigurationSource(
    string? connectionString) : IConfigurationSource
{ 
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new EntityConfigurationProvider(connectionString);
}

public sealed class EntityConfigurationProvider 
    : ConfigurationProvider
{ 
    public string? ConnectionString { get; set; }

    public EntityConfigurationProvider( string? connectionString)
    {
        ConnectionString = connectionString;
    }
    public override void Load()
    {
        using var dbContext = new EntityConfigurationContext(ConnectionString);

        dbContext.Database.EnsureCreated();

        Data = dbContext.EntityConfigurationValues.Any()
            ? dbContext.EntityConfigurationValues.ToDictionary(
                static c => c.Key,
                static c => c.Value, StringComparer.OrdinalIgnoreCase)
            : CreateAndSaveDefaultValues(dbContext);
    }

    static Dictionary<string, string?> CreateAndSaveDefaultValues(
        EntityConfigurationContext context)
    {
        var EntityConfigurationValues = new Dictionary<string, string?>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["WidgetOptions:EndpointId"] = "b3da3c4c-9c4e-4411-bc4d-609e2dcc5c67",
            ["WidgetOptions:DisplayLabel"] = "Widgets Incorporated, LLC.",
            ["WidgetOptions:WidgetRoute"] = "api/widgets"
        };

        context.EntityConfigurationValues.AddRange(
            [.. EntityConfigurationValues.Select(static kvp => new EntityConfigurationValue(kvp.Key, kvp.Value))]);

        context.SaveChanges();

        return EntityConfigurationValues;
    }
}

public static class ConfigurationManagerExtensions
{
    public static ConfigurationManager AddEntityConfiguration(
        this ConfigurationManager manager)
    {
        var connectionString = manager.GetConnectionString("ConfigContext");

        IConfigurationBuilder configBuilder = manager;
        configBuilder.Add(new EntityConfigurationSource(connectionString));

        return manager;
    }
}