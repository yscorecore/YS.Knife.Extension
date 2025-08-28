using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Lock
{

    public interface ILockService
    {
        Task<bool> Lock<T>(string key, T token, TimeSpan timeSpan);

        Task<bool> Update<T>(string key, T token, TimeSpan timeSpan);

        Task<bool> UnLock<T>(string key, T token);

        Task<(bool Exists, T Token)> Query<T>(string key);
    }

    public static class LockServiceExtentions
    {
        public static Task<bool> Lock(this ILockService lockService, string key, TimeSpan timeSpan)
        {
            return lockService.Lock(key, key, timeSpan);
        }

        public static Task<bool> UnLock(this ILockService lockService, string key)
        {
            return lockService.UnLock(key, key);
        }
        public static Task<bool> Update(this ILockService lockService, string key, TimeSpan timeSpan)
        {
            return lockService.Update(key, key, timeSpan);
        }
        public static async Task<bool> GlobalRunOnce(this ILockService lockService, string key, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (await lockService.Lock(key, TimeSpan.MaxValue))
            {
                try
                {
                    action();
                    return true;
                }
                finally
                {
                    await lockService.UnLock(key);
                }
            }
            return false;
        }
        public static async Task WaitFor(this ILockService lockService, string key, int millisecondsDelayInLoop = 100, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(millisecondsDelayInLoop, cancellationToken);
                var (exists, _) = await lockService.Query<string>(key);
                if (!exists)
                {
                    break;
                }
            }
        }
        public static async Task GlobalRunOnceOrWaitFor(this ILockService lockService, string key, Action action, int millisecondsDelayInLoop = 100, CancellationToken cancellationToken = default)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var executed = await lockService.GlobalRunOnce(key, action);
            if (executed == false)
            {
                await lockService.WaitFor(key, millisecondsDelayInLoop, cancellationToken);
            }
        }

        public static async Task RunWithLock(this ILockService lockService, string key, TimeSpan maxTimespan, Action action)
        {
            var success = await lockService.Lock(key, maxTimespan);
            if (success)
            {
                try
                {
                    action?.Invoke();
                }
                finally
                {
                    await lockService.UnLock(key);
                }
            }
        }
        public static Task RunWithLock(this ILockService lockService, string key, Action action)
        {
            return RunWithLock(lockService, key, new TimeSpan(7, 0, 0, 0), action);
        }

        public static async Task RunWithLock(this ILockService lockService, string key, TimeSpan maxTimespan, Task action)
        {
            var success = await lockService.Lock(key, maxTimespan);
            if (success)
            {
                try
                {
                    if (action != null)
                    {
                        await action;
                    }
                }
                finally
                {
                    await lockService.UnLock(key);
                }
            }
        }
        public static Task RunWithLock(this ILockService lockService, string key, Task action)
        {
            return RunWithLock(lockService, key, TimeSpan.MaxValue, action);
        }
    }

}
