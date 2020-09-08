using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BigSister.Commands;
using DSharpPlus.CommandsNext;

namespace BigSister
{
    public static class Bot
    {
        public static async Task RunAsync(DiscordClient botClient)
        {
            RegisterCommands(botClient);
            RegisterEvents(botClient);

            await botClient.ConnectAsync();
            await Task.Delay(-1);
        }

        static void RegisterCommands(DiscordClient botClient)
        {
            var commands = botClient.GetCommandsNext();

            commands.RegisterCommands<FilterCommands>();
        }

        static void RegisterEvents(DiscordClient botClient)
        {
            botClient.MessageCreated += Filter.FilterSystem.BotClient_MessageCreated;
            botClient.MessageUpdated += Filter.FilterSystem.BotClient_MessageUpdated;
            
            // Filter triggered.
            Filter.FilterSystem.FilterTriggered += FilterSystem_FilterTriggered;
        }

        private static void FilterSystem_FilterTriggered(Filter.FilterEventArgs e)
        {

            throw new NotImplementedException();
        }
    }
}
