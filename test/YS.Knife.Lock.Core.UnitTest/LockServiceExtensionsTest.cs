using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using YS.Knife.Lock;
using YS.Knife.Testing;

namespace YS.Lock.Core.UnitTest
{

    public class LockServiceExtensionsTest
    {
        ILockService lockService = Mock.Of<ILockService>();
        [Fact]
        public async Task ShouldCallLockMthodWithTheSameToken()
        {
            var key = Utility.NewPassword();
            var timeSpan = TimeSpan.FromSeconds(10);
            await lockService.Lock(key, timeSpan);
            Mock.Get(lockService).Verify(p => p.Lock(key, key, timeSpan), Times.Once);
        }
        [Fact]
        public async Task ShouldCallUnLockMthodWithTheSameToken()
        {
            var key = Utility.NewPassword();
            await lockService.UnLock(key);
            Mock.Get(lockService).Verify(p => p.UnLock(key, key), Times.Once);
        }
        [Fact]
        public async Task ShouldCallUpdateMthodWithTheSameToken()
        {
            var key = Utility.NewPassword();
            var timeSpan = TimeSpan.FromSeconds(10);
            await lockService.Update(key, timeSpan);
            Mock.Get(lockService).Verify(p => p.Update(key, key, timeSpan), Times.Once);
        }
        [Fact]
        public void ShouldOnlyRunOnceWhenInvokeConcurrent()
        {
            var key = Utility.NewPassword();
            var isFirstTime = true;
            var actionExecutionCount = 0;
            var locker = new object();
            var locker2 = new object();
            Mock.Get(lockService).Setup(p => p.Lock(key, key, It.IsAny<TimeSpan>()))
                .ReturnsAsync(() =>
                {
                    lock (locker)
                    {
                        var isFirst = isFirstTime;
                        isFirstTime = false;
                        return isFirst;
                    }
                });
            var tasks = System.Linq.Enumerable.Range(0, 10).Select(p => lockService.GlobalRunOnce(key, Action)).ToArray();
            Task.WaitAll(tasks);
            int executeActionTaskCount = tasks.Count(p => p.Result);

            executeActionTaskCount.Should().Be(1);
            actionExecutionCount.Should().Be(1);

            void Action()
            {
                lock (locker2)
                {
                    Task.Delay(10).Wait();
                    actionExecutionCount++;
                }
            }
        }
        [Fact]
        public async Task ShouldWaitForReleaseLock()
        {
            var key = Utility.NewPassword();
            var sequence = 1;
            // true, true, true, false, false,...
            Mock.Get(lockService).Setup(p => p.Query<string>(key))
                .ReturnsAsync(() => (sequence++ <= 3, key));
            var start = DateTime.Now;
            await lockService.WaitFor(key);

            var timespan = DateTime.Now - start;
            timespan.TotalMilliseconds.Should().BeGreaterOrEqualTo(400);
        }
        [Fact]
        public void ShouldGlobalRunOnceAndWaitForReleaseLock()
        {
            var key = Utility.NewPassword();
            var isFirstTime = true;

            var actionExecutionCount = 0;
            var locker2 = new object();
            var locker = new object();
            Mock.Get(lockService).Setup(p => p.Lock(key, key, It.IsAny<TimeSpan>()))
               .ReturnsAsync(() =>
               {
                   lock (locker)
                   {
                       var isFirst = isFirstTime;
                       isFirstTime = false;
                       return isFirst;
                   }
               });
            Mock.Get(lockService).Setup(p => p.Query<string>(key))
                .ReturnsAsync(() => (actionExecutionCount == 0, key));

            var tasks = System.Linq.Enumerable.Range(0, 10).Select(p => lockService.GlobalRunOnceOrWaitFor(key, Action)).ToArray();
            Task.WaitAll(tasks);

            actionExecutionCount.Should().Be(1);

            void Action()
            {
                lock (locker2)
                {
                    Task.Delay(10).Wait();
                    actionExecutionCount++;
                }
            }
        }
    }
}
