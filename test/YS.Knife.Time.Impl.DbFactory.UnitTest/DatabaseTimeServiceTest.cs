using YS.Knife.Time.Core.UnitTest;

namespace YS.Knife.Time.Impl.EFDatabaseFacade.UnitTest
{
    [Collection("DockerCompose")]
    public class DatabaseTimeServiceTest : TimeServiceTest
    {
        public DatabaseTimeServiceTest()
        {
            this.GetService<TestDbContext>().Database.EnsureCreated();
        }
    }
}
