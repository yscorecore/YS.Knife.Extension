using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace YS.Knife.Job
{
    public interface IJob
    {
        string Type { get; }
    }
    public interface IJob<T>:IJob
    {
        Task<(bool Success, string ErrorMessage)> ExecuteArgument(T argument);
    }

    public abstract class BaseJob<T> : IJob<T>
    {
        public static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public string Type => typeof(T).Name;

        //public Task<(bool Success, string ErrorMessage)> Execute(string? argument)
        //{
        //    var argObject = argument.AsJsonObject<T>(Options);
        //    return ExecuteArgument(argObject);
        //}
        public abstract Task<(bool Success, string ErrorMessage)> ExecuteArgument(T argument);

    }
   
    //public static class JobServiceExtensions
    //{
        //public static JsonSerializerOptions Options = new JsonSerializerOptions()
        //{
        //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        //};
        //public static Task<bool> RunJob<T>(this IJobService jobService, T data, DateTime? latestExecutionTime = default, int? increaseSeconds = default)
        //{
        //    return jobService.RunJobAt(DateTime.Now, data, latestExecutionTime, increaseSeconds);
        //}
        //public static Task<bool> RunJobAt<T>(this IJobService jobService, DateTime executeTime, T data, DateTime? latestExecutionTime = default, int? increaseSeconds = default)
        //{
        //    return jobService.AddJob(new JobInfo()
        //    {
        //        Key = (data as IKeyJobInfo)?.Key,
        //        Type = typeof(T).Name,
        //        Argument = JsonSerializer.Serialize(data, Options),
        //        ExecutionTime = executeTime,
        //        LatestExecutionTime = latestExecutionTime,
        //        IncreaseSeconds = increaseSeconds
        //    });
        //}


        //public static Task<bool> RunJobs<T>(this IJobService jobService, IEnumerable<T> datas, DateTime? latestExecutionTime = default, int? increaseSeconds = default)
        //{
        //    return jobService.RunJobsAt(DateTime.Now, datas, latestExecutionTime, increaseSeconds);
        //}
        //public static Task<bool> RunJobsAt<T>(this IJobService jobService, DateTime executeTime, IEnumerable<T> datas, DateTime? latestExecutionTime = default, int? increaseSeconds = default)
        //{
        //    return jobService.AddJob(datas.Select(data => new JobInfo()
        //    {
        //        Key = (data as IKeyJobInfo)?.Key,
        //        Type = typeof(T).Name,
        //        Argument = JsonSerializer.Serialize(data, Options),
        //        ExecutionTime = executeTime,
        //        LatestExecutionTime = latestExecutionTime,
        //        IncreaseSeconds = increaseSeconds
        //    }).ToArray());
        //}
   // }
}
