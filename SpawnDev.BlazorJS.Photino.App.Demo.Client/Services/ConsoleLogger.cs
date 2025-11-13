namespace SpawnDev.BlazorJS.Photino.App.Demo.Client.Services
{
    public interface IConsoleLogger
    {
        Task LogAsync(string message);
        void Log(string message);
    }

    public class ConsoleLogger : IConsoleLogger
    {
        public ConsoleLogger()
        {
            var nmt = true;
        }
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public Task LogAsync(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}
