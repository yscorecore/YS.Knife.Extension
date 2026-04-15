namespace YS.Knife.Job
{
    public interface IJobService
    {
        Task<bool> EcecOrPush<T>(string type, T data);
        Task<bool> PushJob<T>(string type, params T[] data);
    }
  

    public static class JobServiceExtensions
    {
        public static Task<bool> ExecObject<T>(this IJobService jobService, T data)
        {
            return Task.FromResult(true);
        }

        
    }
  
}
