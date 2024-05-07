using Microsoft.EntityFrameworkCore;

namespace Config;

public class ConfigValue
{
    public string Id { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ConfigContext : DbContext
{
    public ConfigContext(DbContextOptions<ConfigContext> options) : base(options)
    {
    }

    public DbSet<ConfigValue> ConfigValues => Set<ConfigValue>();
}

public class ConfigProvider : ConfigurationProvider
{
    public ConfigProvider(Action<DbContextOptionsBuilder> optionsAction)
    {
        OptionsAction = optionsAction;
        var root = new ConfigurationReloadToken();
    }

    Action<DbContextOptionsBuilder> OptionsAction { get; }

    public override void Load()
    {
        var builder = new DbContextOptionsBuilder<ConfigContext>();

        OptionsAction(builder);

        using (var dbContext = new ConfigContext(builder.Options))
        {
            if (dbContext == null || dbContext.ConfigValues == null)
            {
                throw new Exception("Null DB context");
            }
            dbContext.Database.EnsureCreated();

            Data = dbContext.ConfigValues.Any()
                ? CreateAndSaveDefaultValues(dbContext)
                : dbContext.ConfigValues.ToDictionary(c => c.Id, c => c.Value);
        }
    }

    private static IDictionary<string, string> CreateAndSaveDefaultValues(
        ConfigContext dbContext)
    {
        Console.WriteLine("CreateAndSaveDefaultValues");
        // Quotes (c)2005 Universal Pictures: Serenity
        // https://www.uphe.com/movies/serenity-2005
        var configValues =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                    { "quote1", "I aim to misbehave." },
                    { "quote2", "I swallowed a bug." },
                    { "quote3", "You can't stop the signal, Mal." }
            };

        if (dbContext == null || dbContext.ConfigValues == null)
        {
            throw new Exception("Null DB context");
        }

        dbContext.ConfigValues.AddRange(configValues
            .Select(kvp => new ConfigValue
            {
                Id = kvp.Key,
                Value = kvp.Value
            })
            .ToArray());

        dbContext.SaveChanges();

        return configValues;
    }
}

public class ConfigSource : IConfigurationSource
{
    private readonly Action<DbContextOptionsBuilder> _optionsAction;

    public ConfigSource(Action<DbContextOptionsBuilder> optionsAction) => _optionsAction = optionsAction;

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new ConfigProvider(_optionsAction);
}

public static class ConfigExtensions
{
    // 扩展
    public static IConfigurationBuilder AddDbConfig(
               this IConfigurationBuilder builder,
               Action<DbContextOptionsBuilder> optionsAction)
    {
        return builder.Add(new ConfigSource(optionsAction));
    }
}