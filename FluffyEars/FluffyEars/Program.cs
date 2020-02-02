// Program.cs

namespace FluffyEars
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            bot.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
