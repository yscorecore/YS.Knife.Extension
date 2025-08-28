using System.Globalization;
using YS.Knife.Hosting;
using YS.Knife.Lock;
using YS.Knife.Testing;

namespace YS.Lock
{
    public abstract class LockServiceTest : KnifeHost
    {
        public LockServiceTest()
        {
            this.lockService = this.GetService<ILockService>();
        }
        private readonly ILockService lockService;

        [Fact]
        public async Task CanLockSimpleTypes()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key + "string", "", TimeSpan.FromSeconds(2));
            await lockService.Lock(key + "long", 1L, TimeSpan.FromSeconds(2));
            await lockService.Lock(key + "dateTime", DateTime.Now, TimeSpan.FromSeconds(2));
        }


        [Fact]
        public async Task ShouldReturnTrueWhenLockGivenNewKey()
        {
            var key = Utility.NewPassword(16);
            var res = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            res.Should().BeTrue();
        }
        [Fact]
        public async Task ShouldReturnFalseWhenLockGivenExistsKey()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            var res2 = await lockService.Lock(key, "token2", TimeSpan.FromSeconds(2));
            res2.Should().BeFalse();
        }


        [Fact]
        public async Task ShouldNotModifyExpiryWhenReLockFailure()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var res2 = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            res2.Should().BeFalse();
            await Task.Delay(800);
            var res3 = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            res3.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnTrueWhenLockGivenExpiredKey()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(2200);
            var res2 = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            res2.Should().BeTrue();
        }

        [Fact]
        public void ShouldOnlyOneSuccessWhenConcurrenLock()
        {
            DateTime dateTime = DateTime.Now;
            int loopCount = 15;
            var successCount = Enumerable.Range(0, 4).AsParallel()
                                .Select(async p => await RunStep(p, dateTime, loopCount))
                                .Sum(p => p.Result);
            successCount.Should().Be(loopCount);

        }
        private async Task<int> RunStep(int taskId, DateTime start, int count)
        {
            int successCount = 0;
            for (int i = 0; i < count; i++)
            {
                var key = start.AddSeconds(i).ToString("HHmmss", CultureInfo.InvariantCulture);
                var success = await lockService.Lock(key, "token", TimeSpan.FromMinutes(2));
                successCount += Convert.ToInt32(success);
                if (success)
                {
                    Console.WriteLine($"Task {taskId} locked {key}.");
                }
                await Task.Delay(1000);
            }
            return successCount;
        }

        [Fact]
        public async Task ShouldReturnFalseWhenUnlockNotExistsKey()
        {
            var key = Utility.NewPassword(16);
            var res = await lockService.UnLock(key, "token");
            res.Should().BeFalse();
        }
        [Fact]
        public async Task ShouldReturnTrueWhenUnlockGivenExistsKeyAndValidToken()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            var res = await lockService.UnLock(key, "token");
            res.Should().BeTrue();
        }
        [Fact]
        public async Task ShouldReturnFalseWhenUnlockGivenExistsKeyAndInvalidToken()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            var res = await lockService.UnLock(key, "invalidtoken");
            res.Should().BeFalse();
        }
        [Fact]
        public async Task ShouldReturnTrueWhenLockAfterUnlock()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await lockService.UnLock(key, "token");
            var res = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            res.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnFalseWhenUpdateGivenNoExistsKey()
        {
            var key = Utility.NewPassword(16);
            var res = await lockService.Update(key, "token", TimeSpan.FromSeconds(2));
            res.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldReturnFalseWhenUpdateGivenExpiredKey()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(1));
            await Task.Delay(1500);
            var res = await lockService.Update(key, "token", TimeSpan.FromSeconds(2));
            res.Should().BeFalse();
        }
        [Fact]
        public async Task ShouldReturnTrueWhenUpdateGivenExistsKeyAndInvalidToken()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var res = await lockService.Update(key, "invalidtoken", TimeSpan.FromSeconds(2));
            res.Should().BeFalse();

        }
        [Fact]
        public async Task ShouldReturnTrueWhenUpdateGivenExistsKeyAndValidToken()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var res = await lockService.Update(key, "token", TimeSpan.FromSeconds(2));
            res.Should().BeTrue();
            await Task.Delay(1500);
            var canReLock = await lockService.Lock(key, "token2", TimeSpan.FromSeconds(2));
            canReLock.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldReturnNoExistsWhenQueryGivenNewKey()
        {
            var key = Utility.NewPassword(16);
            var (exists, token) = await lockService.Query<string>(key);
            exists.Should().BeFalse();
            token.Should().Be(default);
        }

        [Fact]
        public async Task ShouldReturnTokenWhenQueryGivenExistsKey()
        {
            var key = Utility.NewPassword(16);
            var time = DateTime.Now;
            await lockService.Lock(key, time, TimeSpan.FromSeconds(2));
            var (exists, token) = await lockService.Query<DateTime>(key);
            exists.Should().BeTrue();
            token.Should().Be(time);
        }

        [Fact]
        public async Task ShouldNotAffectExpiryWhenQueryGivenExistsKey()
        {
            var key = Utility.NewPassword(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var (exists, _) = await lockService.Query<string>(key);
            exists.Should().BeTrue();
            await Task.Delay(800);
            var (exists2, _) = await lockService.Query<string>(key);
            exists2.Should().BeFalse();
        }
    }
}
