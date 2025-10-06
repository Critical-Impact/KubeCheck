using k8s;
using KubeCheck.Models;

namespace KubeCheck.Services;

public class KubernetesClientFactory
{
    private readonly EnvironmentInfo _environmentInfo;

    public KubernetesClientFactory(EnvironmentInfo environmentInfo)
    {
        _environmentInfo = environmentInfo;
    }

    public IKubernetes CreateClient()
    {
        KubernetesClientConfiguration config;
        if (_environmentInfo.KubeConfigPath != null)
        {
            config = KubernetesClientConfiguration.BuildConfigFromConfigFile(_environmentInfo.KubeConfigPath);
        }
        else
        {
            config = KubernetesClientConfiguration.InClusterConfig();
        }

        return new Kubernetes(config);
    }
}