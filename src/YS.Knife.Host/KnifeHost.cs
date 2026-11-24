using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YS.Knife.Hosting
{
    [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class InjectAttribute : System.Attribute
    {
        public InjectAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            this.Lifetime = lifetime;
        }
        public ServiceLifetime Lifetime { get; set; }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]

    public class InjectConfigurationAttribute : System.Attribute
    {
        public InjectConfigurationAttribute(string configurationKey)
        {
            this.ConfigurationKey = configurationKey;
        }
        public string ConfigurationKey { get; set; }
    }

    public class KnifeHost : IDisposable, IServiceProvider
    {
        public KnifeHost() : this(Array.Empty<string>())
        {
        }

        public KnifeHost(IDictionary<string, object> args)
            : this(args.Select(p => $"/{p.Key}={p.Value}").ToArray())
        {
        }

        public KnifeHost(string[] args)
        {
            this.host = CreateHost(args ?? Array.Empty<string>());
        }



        private readonly IHost host;

        public object? GetService(Type serviceType)
        {
            return host.Services.GetService(serviceType);
        }

        public T? GetService<T>()
        {
            return host.Services.GetService<T>();
        }




        protected virtual IHost CreateHost(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            this.OnConfigureLogging(builder.Logging);
            this.OnConfigureAppConfiguration(builder.Configuration);
            this.InjectInternalConfigurations(builder.Configuration);

            builder.Services.AddAllKnifeServices(builder.Configuration, ShouldFilterType);
            this.OnConfigureCustomService(builder.Services);
            this.OnConfigureInternalServices(builder.Services);
            this.InjectInternalServices(builder.Services);

            return builder.Build();
        }


        protected virtual void OnConfigureInternalServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(typeof(KnifeHost), this);
            serviceCollection.AddSingleton(typeof(IServiceCollection), serviceCollection);
        }

        protected virtual void OnConfigureCustomService(IServiceCollection serviceCollection)
        {
        }

        protected virtual bool ShouldFilterType(Type type)
        {
            return false;
        }

        private void InjectInternalServices(IServiceCollection serviceCollection)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                               BindingFlags.Static;
            var fields = this.GetType().GetFields(bindingFlags)
                .Where(p => Attribute.IsDefined(p, typeof(InjectAttribute)));
            foreach (var field in fields)
            {
                InjectAttribute inject = field.GetCustomAttribute<InjectAttribute>()!;
                serviceCollection.Add(new ServiceDescriptor(field.FieldType, (sp) => field.GetValue(this)!,
                    inject.Lifetime));
            }

            // props
            var props = this.GetType().GetProperties(bindingFlags)
                .Where(p => Attribute.IsDefined(p, typeof(InjectAttribute)));
            foreach (var prop in props)
            {
                InjectAttribute inject = prop.GetCustomAttribute<InjectAttribute>()!;
                serviceCollection.Add(new ServiceDescriptor(prop.PropertyType, (sp) => prop.GetValue(this)!,
                    inject.Lifetime));
            }
        }

        private void InjectInternalConfigurations(IConfigurationBuilder configurationBuilder)
        {
            Dictionary<string, string?> configurationData = new Dictionary<string, string?>();
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Static;


            foreach (var field in this.GetType().GetFields(bindingFlags).Select(p => new { Field = p, Attr = p.GetCustomAttribute<InjectConfigurationAttribute>() }))
            {
                AppendConfigurationDataValue(configurationData, field.Attr?.ConfigurationKey, field.Field.GetValue(this));
            }


            foreach (var prop in this.GetType().GetProperties(bindingFlags).Select(p => new { Prop = p, Attr = p.GetCustomAttribute<InjectConfigurationAttribute>() }))
            {
                AppendConfigurationDataValue(configurationData, prop.Attr?.ConfigurationKey, prop.Prop.GetValue(this));
            }


            if (configurationData.Count > 0)
            {
                configurationBuilder.AddInMemoryCollection(configurationData);
            }
        }



        private void AppendConfigurationDataValue(Dictionary<string, string?> data, string? key, object? value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (value is IDictionary dic)
            {
                foreach (var subKey in dic.Keys)
                {
                    string childKey = $"{key}:{subKey}";
                    AppendConfigurationDataValue(data, childKey, dic[subKey]);
                }
            }

            data[key] = $"{value}";
        }

        protected virtual void OnConfigureLogging(ILoggingBuilder loggingBuilder)
        {
        }

        protected virtual void OnConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
        {
        }


        public void Run()
        {
            this.PrintDebugInfo();
            this.host.Run();

        }

        private void PrintDebugInfo()
        {
            var hostEnv = this.host.Services.GetRequiredService<IHostEnvironment>();
            if (hostEnv.IsDevelopment())
            {
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });
                var logger = loggerFactory.CreateLogger<KnifeHost>();
                // only in development mode
                LogConfigurationDebugInfo(logger);
                LogDependencyInjectionServicesInfo(logger);
            }
        }

        private void LogConfigurationDebugInfo(ILogger logger)
        {
            var configRoot = this.host.Services.GetRequiredService<IConfiguration>() as IConfigurationRoot;
            if (configRoot == null) return;
            StringBuilder content = new StringBuilder();
            content.AppendLine("===================Configuration Values===============");
            content.AppendLine(configRoot.GetDebugView());
            content.AppendLine("======================================================");
            logger.LogDebug(content.ToString());
        }

        private void LogDependencyInjectionServicesInfo(ILogger logger)
        {
            var serviceCollection = this.host.Services.GetRequiredService<IServiceCollection>();
            if (serviceCollection == null) return;
            StringBuilder content = new StringBuilder();
            content.AppendLine("====================Injection Services================");
            content.AppendLine(GetDebugView(serviceCollection));
            content.AppendLine("======================================================");
            logger.LogDebug(content.ToString());
        }

        private string GetDebugView(IServiceCollection serviceCollection)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"a total of {serviceCollection.Count} services were injected.");
            var sequence = NewSequence();
            foreach (var sp in serviceCollection)
            {
                if (sp.ImplementationType != null)
                {
                    stringBuilder.AppendLine(
                        $"[{sequence():000}] [{sp.Lifetime}] {sp.ServiceType} from type {sp.ImplementationType}");
                }
                else if (sp.ImplementationInstance != null)
                {
                    stringBuilder.AppendLine(
                        $"[{sequence():000}] [{sp.Lifetime}] {sp.ServiceType} from instance");
                }
                else if (sp.ImplementationFactory != null)
                {
                    stringBuilder.AppendLine(
                        $"[{sequence():000}] [{sp.Lifetime}] {sp.ServiceType} from factory {sp.ImplementationFactory.Method}");
                }
            }

            return stringBuilder.ToString();

            Func<int> NewSequence()
            {
                int value = 1;
                return () => value++;
            }
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            await this.host.RunAsync(token);

        }

        #region IDisposable Support

        private bool disposedValue; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    host.Dispose();
                }

                disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
