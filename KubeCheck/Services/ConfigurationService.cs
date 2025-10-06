using KubeCheck.Models;

namespace KubeCheck.Services;

public class ConfigurationService
{
    private readonly string _filePath;

    public ConfigurationService(EnvironmentInfo environmentInfo)
    {
        var configDirectory = new DirectoryInfo(environmentInfo.ConfigPath);
        if (!configDirectory.Exists)
        {
            configDirectory.Create();
        }
        _filePath = Path.Join(environmentInfo.ConfigPath, "configuration.json");
    }

    public Configuration Load()
    {
        if (!File.Exists(_filePath))
        {
            var defaultConfig = new Configuration();
            Save(defaultConfig);
            return defaultConfig;
        }

        var json = File.ReadAllText(_filePath);
        return System.Text.Json.JsonSerializer.Deserialize<Configuration>(json) ?? new Configuration();
    }

    public void Save(Configuration config)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(config);
        File.WriteAllText(_filePath, json);
    }
}