namespace YS.Knife.Task
{
    public record class TaskExecuteResult(bool Success, string Message)
    {
        public static TaskExecuteResult Ok { get; } = new TaskExecuteResult(true, "Success");
        public static TaskExecuteResult FromException(Exception ex)
        {
            return new TaskExecuteResult(false, ex.Message);
        }
        public static TaskExecuteResult Failure(string message)
        {
            return new TaskExecuteResult(false, message);
        }
    }

}
