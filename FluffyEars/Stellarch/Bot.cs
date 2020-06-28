// Bot.cs
// Contains code to...
// 1) Initiate and configure the bot client, CommandsNext extension, and Interactivity extension.
// 2) Register events and commands.
// 3) Connect the bot to the Discord servers.

using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;

namespace Stellarch
{
    static class Bot
    {
        private static string authKey;
        private static Audit audit;

        /// <summary>Begin running the bot.</summary>
        internal static async Task RunAsync(string authKey)
        {
            DiscordConfiguration botConfig;
            DiscordClient botClient;

            CommandsNextConfiguration commandsNextConfig;

            InteractivityConfiguration interactivityConfiguration;


            // --------------------------------
            // Initiate the bot client.

            botConfig = new DiscordConfiguration
            {
                Token = authKey,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                // WebSocketClientFactory -- NEED TO SET THIS
                UseInternalLogHandler = true,
                // VVV NEED TO LOAD SETTINGS THAT CHANGE THIS
                LogLevel = LogLevel.Critical | LogLevel.Debug | LogLevel.Error | LogLevel.Info | LogLevel.Warning,
            };

            botClient = new DiscordClient(botConfig);


            // --------------------------------
            // Initiate the CommandsNext extension.

            commandsNextConfig = new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false,
                EnableDms = true,
                DmHelp = true
                // StringPrefixes <- CHANGE THIS WITH SETTINGS
                // PrefixResolver <- ADD THIS.
                // UseDefaultCommandHandler <- CONSIDER ADDING OWN DEFAULT COMMAND HANDLER
            };

            // TODO: INITIATE COMMANDSNEXT MODULE(S) HERE


            // --------------------------------
            // Initiate the Interactivity extension.

            interactivityConfiguration = new InteractivityConfiguration
            {
                PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromMinutes(2)
            };

            botClient.UseInteractivity(interactivityConfiguration);

            // --------------------------------
            // Register Events

            // TODO: actually register the events here.

            // --------------------------------
            // Begin auditing

            audit = new Audit();
            await audit.ConnectAsync();

            // --------------------------------
            // Start the bot already!

            await botClient.ConnectAsync().ConfigureAwait(false);
            await Task.Delay(-1);
        }
    }
}
