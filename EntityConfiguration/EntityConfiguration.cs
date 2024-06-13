using System.Collections;
using System.ComponentModel.DataAnnotations;
using AntDesign.Core.Extensions;
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

public sealed class EntityConfigurationSource : IConfigurationSource
{
    private readonly string? connectionString;
    public EntityConfigurationSource(string? connectionString)
    {
        this.connectionString = connectionString;
    }
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new EntityConfigurationProvider(connectionString);
}

public sealed class EntityConfigurationProvider
    : ConfigurationProvider
{
    public string? ConnectionString { get; set; }
    private readonly ILogger<EntityConfigurationProvider> _logger;


    public EntityConfigurationProvider(string? connectionString)
    {
        ConnectionString = connectionString;
        // 构建配置
        var loggerFactory = LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
        });
        this._logger = loggerFactory.CreateLogger<EntityConfigurationProvider>();

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

    public override void Set(string key, string? value)
    {
        base.Set(key, value);
        using var dbContext = new EntityConfigurationContext(ConnectionString);
        var item = new EntityConfigurationValue(key, value);
        try
        {
            _logger.LogDebug($"add or update item => key:{key} value:{value} ");
            if (dbContext.EntityConfigurationValues.Any(e => e.Key == key))
            {
                _logger.LogDebug("已存在，需要更新");
                dbContext.EntityConfigurationValues.Update(item);
            }
            else
            {
                _logger.LogDebug("不存在，需要新增");
                dbContext.EntityConfigurationValues.Add(item);
            }
            dbContext.SaveChanges();
        }
        catch (System.Exception e)
        {
            _logger.LogError($"error type: {e.GetType()}  msg: {e.Message}");
            return;
        }

    }

    static Dictionary<string, string?> CreateAndSaveDefaultValues(
        EntityConfigurationContext context)
    {
        var EntityConfigurationValues = new Dictionary<string, string?>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["WidgetOptions:EndpointId"] = "b3da3c4c-9c4e-4411-bc4d-609e2dcc5c67",
            ["WidgetOptions:DisplayLabel"] = "Widgets Incorporated, LLC.",
            ["WidgetOptions:WidgetRoute"] = "api/widgets",
            ["version"] = DateTime.Now.ToLongTimeString()
        };

        context.EntityConfigurationValues.AddRange(
            [.. EntityConfigurationValues.Select(static kvp => new EntityConfigurationValue(kvp.Key, kvp.Value))]);

        context.SaveChanges();

        return EntityConfigurationValues;
    }
}

public static class ConfigurationManagerExtensions
{
    public static ConfigurationManager AddEntityConfiguration(this ConfigurationManager manager)
    {
        var connectionString = manager.GetConnectionString("ConfigContext");
        (manager as IConfigurationBuilder).Add(new EntityConfigurationSource(connectionString));
        return manager;
    }
}