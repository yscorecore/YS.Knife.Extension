using YS.Knife.Testing;

namespace YS.Knife.Time.Impl.Redis.UnitTest
{
    [CollectionDefinition("DockerCompose")]
    public class DockerComposeFixture : IAsyncLifetime, ICollectionFixture<DockerComposeFixture>
    {
        public Task DisposeAsync()
        {
            DockerCompose.Down();
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            var availablePort = Utility.GetAvailableTcpPort(9999);
            StartContainer(availablePort);
            SetConnectionString(availablePort);
            await Task.Delay(20 * 1000);//等待容器启动
        }
        private static void SetConnectionString(uint port)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__TestDb", $"Server=localhost;Port={port};User Id=postgres;Password=mypassword;Database=testdb;IncludeErrorDetail=true");
        }
        private static void StartContainer(uint port)
        {
            var envs = new Dictionary<string, object>
            {
                ["DBPORT"] = port,
            };
            DockerCompose.Up(envs);
        }


    }
}
