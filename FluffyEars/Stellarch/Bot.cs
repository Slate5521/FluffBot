// Bot.cs
// Initiates commands and events related to the bot and handles event handlers from custom classes. It also runs the bot and maintains a connection.
//
// EMIKO

using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BigSister.Commands;
using DSharpPlus.CommandsNext;
using System.Timers;
using BigSister.Reminders;
using BigSister.MentionSnooper;

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

            commands.RegisterCommands<FilterCommands>();
            commands.RegisterCommands<ReminderCommands>();
            commands.RegisterCommands<RoleRequestCommands>();
            commands.RegisterCommands<MentionCommands>();
            commands.RegisterCommands<SettingsCommands>();
            commands.RegisterCommands<RequestedCommands>();
        }

        static void RegisterEvents(DiscordClient botClient)
        {
            // ----------------
            // Filter
            botClient.MessageCreated += Filter.FilterSystem.BotClient_MessageCreated;
            botClient.MessageUpdated += Filter.FilterSystem.BotClient_MessageUpdated;
            
            // Filter triggered.
            Filter.FilterSystem.FilterTriggered += FilterSystem_FilterTriggered;

            // ----------------
            // Snooper
            botClient.MessageCreated += MentionSnooper.MentionSnooper.BotClientMessageCreated;
            botClient.MessageUpdated += MentionSnooper.MentionSnooper.BotClientMessageUpdated;

            // ----------------
            // Reminder timer
            reminderTimer.Elapsed += ReminderSystem.ReminderTimer_Elapsed;

            // ----------------
            // Rimboard
            botClient.MessageReactionAdded += Rimboard.RimboardSystem.BotClientMessageReactionAdded;
            botClient.MessageReactionsCleared += Rimboard.RimboardSystem.BotClientMessageReactionsCleared;
            botClient.MessageDeleted += Rimboard.RimboardSystem.BotClientMessageDeleted;

            // ----------------
            // RoleRequest

            botClient.MessageReactionAdded += RoleRequest.RoleRequestSystem.MessageReactionAdded;
            botClient.MessageReactionRemoved += RoleRequest.RoleRequestSystem.MessageReactionRemoved;

            // ----------------
            // Auditing

            var commandsNext = botClient.GetCommandsNext();

            commandsNext.CommandExecuted += Loggah.AuditSystem.Bot_CommandExecuted;
        }

        private static void FilterSystem_FilterTriggered(Filter.FilterEventArgs e)
        {

            throw new NotImplementedException();
        }
    }
}
