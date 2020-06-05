// Bot.cs
// This is basically just the bot instance. I also put most of the the event handlers within this class.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluffyEars.Reminders;
using FluffyEars.BadWords;
using DSharpPlus.CommandsNext.Exceptions;
using FluffyEars.Commands;

namespace FluffyEars
{
    class Bot
    {
        /// <summary>The bot client.</summary>
        public static DiscordClient BotClient;
        /// <summary>CommandNextModule</summary>
        public CommandsNextExtension Commands;

        public Bot() { }

        #region Public Methods

        /// <summary>Start this shit up!</summary>
        public async Task RunAsync()
        {
            // Load shit.
            string authKey = LoadConfig();

            DiscordConfiguration botConfig = new DiscordConfiguration
            {
                Token = authKey,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Critical | LogLevel.Debug | LogLevel.Error | LogLevel.Info | LogLevel.Warning,
            };

            BotClient = new DiscordClient(botConfig);

            var commandConfig = new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false,
                EnableDms = true
            };

            Commands = BotClient.UseCommandsNext(commandConfig);

            Commands.RegisterCommands<ConfigCommands>();
            Commands.RegisterCommands<FilterCommands>();
            Commands.RegisterCommands<ReminderCommands>();
            Commands.RegisterCommands<RequestedCommands>();
            Commands.RegisterCommands<FrozenCommands>();

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            BotClient.MessageCreated += FilterSystem.BotClient_MessageCreated;
            BotClient.MessageCreated += FrozenCommands.BotClient_MessageCreated;
            BotClient.MessageUpdated += FilterSystem.BotClient_MessageUpdated;
            BotClient.ClientErrored += BotClient_ClientErrored;
            BotClient.Heartbeated += ReminderSystem.BotClient_Heartbeated;

            BotClient.Ready += BotClient_Ready;

            FilterSystem.FilterTriggered += BotClient_FilterTriggered;

            await BotClient.ConnectAsync();
            await Task.Delay(-1);
        }

        public static async Task NotifyFilterChannel(DiscordEmbed embed, string text = @"")
        {
            var auditChannel = await BotClient.GetChannelAsync(BotSettings.FilterChannelId);

            await auditChannel.SendMessageAsync
                (
                    content: text == String.Empty ? null : text,
                    embed: embed
                ).ConfigureAwait(false);
        }

        public static async Task NotifyInvalidPermissions(Role requiredRole, string command, DiscordChannel channel, DiscordMember caller)
        {
            var deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                (
                    title: "Invalid Permissions",
                    description: ChatObjects.GetErrMessage($"Hello {caller.Mention}, you do not have the permissions required to use that command..."),
                    color: ChatObjects.ErrColor
                ));

            deb.AddField(@"Command", command, true);
            deb.AddField(@"Role required", requiredRole.ToName(), true);

            await channel.SendMessageAsync(embed: deb.Build());
        }

        #endregion Public Methods
        // ################################
        #region Private Methods

        private string LoadConfig()
        {
            var authKey = String.Empty;

            // =================
            // Load Authkey
            Console.WriteLine(@"Loading bot config:");

            Console.Write("\tAuthkey");
            using (var fs = File.OpenRead(@"authkey"))
            {
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    authKey = sr.ReadToEnd();
                }
            }

            if (authKey != String.Empty)
            {
                Console.WriteLine(@"... Loaded.");
            }

            // =================
            // Bot settings

            Console.Write("\tSettings");

            if (BotSettings.CanLoad())
            {
                BotSettings.Load();
                Console.WriteLine(@"... Loaded!");
            }
            else
            {
                BotSettings.Default();
                Console.WriteLine(@"... Not found. Loading default values.");
            }

            // ---
            Console.Write("\tReminders");

            if (ReminderSystem.CanLoad())
            {
                ReminderSystem.Load();
                Console.WriteLine(@"... Loaded!");
            }
            else
            {
                ReminderSystem.Default();
                Console.WriteLine(@"... Not found. Instantiating default values.");
            }

            // ---
            Console.Write("\tBadwords");

            if (FilterSystem.CanLoad())
            {
                FilterSystem.Load();
                Console.WriteLine(@"... Loaded!");
            }
            else
            {
                FilterSystem.Default();
                Console.WriteLine(@"... Not found. Instantiating default values.");
            }

            // ---
            Console.Write("\tExcludes");

            if (Excludes.CanLoad())
            {
                Excludes.Load();
                Console.WriteLine(@"... Loaded!");
            }
            else
            {
                Excludes.Default();
                Console.WriteLine(@"... Not found. Instantiating default values.");
            }

            return authKey;
        }

        #endregion Private Methods
        // ################################
        #region Event Listeners

        private async Task BotClient_Ready(ReadyEventArgs e)
        {
            if (BotSettings.StartMessageEnabled)
            {
                try
                {
                    await BotClient.SendMessageAsync(await BotClient.GetChannelAsync(326892498096095233),
                        ChatObjects.GetNeutralMessage(@"I'm a bunny."));
                    await BotClient.SendMessageAsync(await BotClient.GetChannelAsync(214523379766525963),
                        ChatObjects.GetNeutralMessage(@"I'm a bunny."));
                }
                catch { }
            }
            
            SelfAudit.LogSomething
                (
                    who: BotClient.CurrentUser, 
                    messageUrl: @"n/a", 
                    description: @"The bot has started.", 
                    command: @"Startup", 
                    arguments: String.Empty, 
                    color: DiscordColor.IndianRed
                ).ConfigureAwait(false).GetAwaiter().GetResult();
        }


        private Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (!(e.Exception is CommandNotFoundException) &&
                !(e.Exception is ChecksFailedException))
            {
                e.Context.Client.DebugLogger
                    .LogMessage(LogLevel.Error, "FloppyEars", $"Exception: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            }

            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger
                .LogMessage(LogLevel.Info, "FloppyEars", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            SelfAudit.LogSomething
                (
                    who: e.Context.Member, 
                    messageUrl: ChatObjects.GetMessageUrl(e.Context.Message),
                    description: @"A command was executed.",
                    command: e.Command.QualifiedName,
                    arguments: e.Context.Message.Content,
                    color: DiscordColor.Yellow
                ).ConfigureAwait(false).GetAwaiter().GetResult();

            return Task.CompletedTask;
        }

        private async void BotClient_FilterTriggered(FilterEventArgs e)
            => await FilterSystem.HandleFilterTriggered(e);

        /// <summary>Some shit broke.</summary>
        private Task BotClient_ClientErrored(ClientErrorEventArgs e)
        {
            Console.WriteLine(e.EventName);
            Console.WriteLine(e.Exception.ToString());

            return Task.CompletedTask;
        }

        #endregion Event Listeners
    }
}
