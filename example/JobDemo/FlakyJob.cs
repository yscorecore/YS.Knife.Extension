namespace JobDemo
{
    /// <summary>
    /// 一个前两次执行失败、第三次才成功的任务，用于演示 IJobService 的自动重试能力。
    /// </summary>
    public class FlakyJob
    {
        private static int attempt;

        public Task Run()
        {
            attempt++;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] FlakyJob 执行第 {attempt} 次");

            if (attempt < 5)
            {
                throw new InvalidOperationException($"FlakyJob 第 {attempt} 次执行失败，触发自动重试。");
            }

            Console.WriteLine("FlakyJob 重试后执行成功。");
            return Task.CompletedTask;
        }
    }
}
