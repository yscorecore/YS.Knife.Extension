using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.Testing;

namespace YS.Knife.Lock.Impl.Redis.UnitTest
{
    [CollectionDefinition("DockerCompose")]
    public class DockerComposeFixture : IAsyncLifetime, ICollectionFixture<DockerComposeFixture>
    {
        public Task DisposeAsync()
        {
            DockerCompose.Down();
            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            var availablePort = Utility.GetAvailableTcpPort(6379);
            StartContainer(availablePort);
            SetConnectionString(availablePort);
            return Task.CompletedTask;
        }
        private static void SetConnectionString(uint port)
        {
            Environment.SetEnvironmentVariable("Redis__ConnectionString", $"localhost:{port}");
        }
        private static void StartContainer(uint port)
        {
            DockerCompose.Up(new Dictionary<string, object>
            {
                ["REDIS_PORT"] = port
            });
        }
    }
}
