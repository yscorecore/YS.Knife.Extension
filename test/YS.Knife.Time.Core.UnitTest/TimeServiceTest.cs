using YS.Knife.Hosting;

namespace YS.Knife.Time.Core.UnitTest
{
    public abstract class TimeServiceTest : KnifeHost
    {
        [Fact]
        public async Task ShouldGetCurrentTime()
        {
            var service = this.GetService<ITimeService>();
            var time = await service.Now();
            time.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(60));
        }
    }
}
