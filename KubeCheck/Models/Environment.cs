using Microsoft.Extensions.Logging;

namespace KubeCheck.Models;

public class EnvironmentInfo
{
    public string ConfigPath { get; }

    public LogLevel LogLevel { get; }

    public string? KubeConfigPath { get; }

    public EnvironmentInfo(string configPath, LogLevel logLevel, string? kubeConfigPath)
    {
        ConfigPath = configPath;
        LogLevel = logLevel;
        KubeConfigPath = kubeConfigPath;
    }

}