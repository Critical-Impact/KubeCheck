using System.Net;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using KubeCheck.Checks;
using KubeCheck.Models;
using KubeCheck.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KubeCheck
{
    class Program
    {
        private static EnvironmentInfo environment;

        static async Task Main(string[] args)
        {
            var configFilePath = Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "/app/config";
            var logLevelString = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information";
            var kubeConfigPath = Environment.GetEnvironmentVariable("KUBECONFIG") ?? null;
            if (!Enum.TryParse<LogLevel>(logLevelString, out var logLevel))
            {
                logLevel = LogLevel.Information;
            }
            environment = new EnvironmentInfo(configFilePath, logLevel, kubeConfigPath);

            using IHost host = CreateHostBuilder(args).UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureLogging(lb =>
            {
                lb.AddConsole();
                lb.SetMinimumLevel(environment.LogLevel);
            }).Build();
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    var dataAccess = Assembly.GetExecutingAssembly();

                    builder.RegisterInstance(environment);
                    builder.RegisterType<ConfigurationService>().SingleInstance();
                    builder.RegisterType<BootService>().AsImplementedInterfaces().AsSelf().SingleInstance();
                    builder.RegisterType<KubernetesClientFactory>().SingleInstance();
                    builder.Register(c => c.Resolve<ConfigurationService>().Load()).SingleInstance();
                    builder.RegisterType<CheckService>().AsSelf().AsImplementedInterfaces().SingleInstance();
                    builder.RegisterType<NotificationService>().AsSelf().AsImplementedInterfaces().SingleInstance();
                    builder.Register(c => c.Resolve<KubernetesClientFactory>().CreateClient()).SingleInstance();
                    builder.RegisterAssemblyTypes(dataAccess).AssignableTo<ICheck>().AsSelf().AsImplementedInterfaces().SingleInstance();
                })
                .ConfigureServices((hostContext, services) =>
                {
                });
    }
}