namespace YS.Knife.TokenMan
{
    public interface ITokenManager
    {
        /// <summary>
        /// 获取Token，如果Token不存在或者过期，则调用tokenFun获取新的Token，并且根据expiredTimeFunc获取过期时间，保存Token到缓存中，返回Token。
        /// </summary>
        /// <typeparam name="T">Token的类型</typeparam>
        /// <param name="name"> Token的名称</param>
        /// <param name="tokenFactory">获取Token的委托函数</param>
        /// <param name="expiredTimeFunc">Token的绝对过期时间</param>
        /// <param name="bufferSeconds">缓冲时间，避免因网络延迟、时钟不同步导致 获取到的Token失效问题</param>
        /// <param name="preloadSeconds">提前预加载的时间，在高并发的场景下，防止Token失效后大量请求等待一个线程请求Token。提前预加载会在Token失效前的一小段（去除了缓冲时间），某请求去请求Token，其余请求还使用旧的Token。提前预加载的时间要大于极端情况下的网络请求时间。</param>
        /// <returns></returns>
        Task<T> GetToken<T>(string name, Func<CancellationToken, Task<T>> tokenFactory, Func<T, DateTimeOffset> expiredTimeFunc, int bufferSeconds = 300, int preloadSeconds = 60, CancellationToken token = default) where T : class;
    }

    public static class ITokenManagerExtensions
    {

        public static Task<T> GetToken<T>(this ITokenManager tokenManager, string name, Func<CancellationToken, Task<T>> tokenFactory, Func<T, TimeSpan> expiredTimeFunc, int bufferSeconds = 300, int preloadSeconds = 60, CancellationToken token = default) where T : class
        {
            return tokenManager.GetToken(name, tokenFactory, t => DateTimeOffset.UtcNow.Add(expiredTimeFunc(t)), bufferSeconds, preloadSeconds, token);
        }
        public static Task<T> GetToken<T>(this ITokenManager tokenManager, string name, Func<CancellationToken, Task<T>> tokenFactory, TimeSpan expiredTime, int bufferSeconds = 300, int preloadSeconds = 60, CancellationToken token = default) where T : class
        {
            return tokenManager.GetToken(name, tokenFactory, t => expiredTime, bufferSeconds, preloadSeconds, token);
        }
        public static Task<T> GetToken<T>(this ITokenManager tokenManager, string name, Func<CancellationToken, Task<T>> tokenFactory, Func<T, DateTime> expiredTimeFunc, int bufferSeconds = 300, int preloadSeconds = 60, CancellationToken token = default) where T : class
        {
            return tokenManager.GetToken(name, tokenFactory, t => (DateTimeOffset)expiredTimeFunc(t), bufferSeconds, preloadSeconds, token);
        }
    }

}
