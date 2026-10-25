using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace YS.Knife.Job.Impl.Hangfire
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.TryAddSingleton<JobStorage>(_ => JobStorage.Current);
            services.AddSingleton<BackgroundJobClient>(sp =>
                new BackgroundJobClient(sp.GetRequiredService<JobStorage>()));
            services.AddSingleton<IBackgroundJobClient>(sp =>
                sp.GetRequiredService<BackgroundJobClient>());
            services.AddSingleton<IRecurringJobManager>(sp =>
                new RecurringJobManager(sp.GetRequiredService<JobStorage>()));
        }
    }
}
