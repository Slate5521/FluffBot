using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BigSister
{
    public static class Bot
    {
        public static async Task RunAsync(DiscordClient botClient)
        {
            // RegisterCommands(botClient);
            // RegisterEvents(botClient);

            await botClient.ConnectAsync();
            await Task.Delay(-1);
        }

        static void RegisterCommands(DiscordClient botClient)
        {
            throw new NotImplementedException();
        }

        static void RegisterEvents(DiscordClient botClient)
        {
            throw new NotImplementedException();
        }
    }
}
