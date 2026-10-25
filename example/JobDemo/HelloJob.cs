namespace JobDemo
{
    public class HelloJob
    {
        public Task SayHi(string label = "")
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Hello from Hangfire job! {label}");
            return Task.CompletedTask;
        }
    }
}
