// Bot.cs
// Initiates commands and events related to the bot and handles event handlers from custom classes. It also runs the bot and maintains a connection.
//
// EMIKO

using System;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Text;
using BigSister.ChatObjects;

namespace BigSister
{
    public static class Bot
    {
        static Timer reminderTimer;

        public static async Task RunAsync(DiscordClient botClient)
        {
            // Configure timer. Set it up BEFORE registering events.
            reminderTimer = new Timer
            {
                Interval = 60000, // 1 minute
                AutoReset = true
            };

            RegisterCommands(botClient);
            RegisterEvents(botClient);

            reminderTimer.Start();

            await botClient.ConnectAsync();
            await Task.Delay(-1);
        }

        static void RegisterCommands(DiscordClient botClient)
        {
            var commands = botClient.GetCommandsNext();

            // Register commands
        }

        static void RegisterEvents(DiscordClient botClient)
        {
            // ----------------
            // Auditing

            var commandsNext = botClient.GetCommandsNext();

            commandsNext.CommandExecuted += AuditSystem.Bot_CommandExecuted;

            // ----------------
            // Reminder Timer

            reminderTimer = new Timer(1000);
            // Event handle reminder timer here.
        }
    }
}
