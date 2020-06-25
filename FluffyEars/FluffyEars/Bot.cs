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
using DSharpPlus.Interactivity;
using FluffyEars.Commands;
using Newtonsoft.Json;

namespace FluffyEars
{
    class Bot
    {
        private struct WebhookInfo
        {
            public ulong ID;
            public string Token;
        }

        /// <summary>The bot client.</summary>
        public static DiscordClient BotClient;
        /// <summary>CommandNext extension.</summary>
        public CommandsNextExtension Commands;
        /// <summary>Interactivity extension.</summary>
        public InteractivityExtension Interactivity { get; set; }
        public static DiscordWebhook Webhook;

        public Bot() { }

        #region Public Methods

        /// <summary>Start this shit up!</summary>
        public async Task RunAsync()
        {
            // Load shit.

            string authKey = LoadConfig(out WebhookInfo webhookInfo);

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

            BotClient.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromMinutes(2)
            });

            Webhook = await BotClient.GetWebhookWithTokenAsync(webhookInfo.ID, webhookInfo.Token);

            Commands = BotClient.UseCommandsNext(commandConfig);

            Commands.RegisterCommands<ConfigCommands>();
            Commands.RegisterCommands<FilterCommands>();
            Commands.RegisterCommands<ReminderCommands>();
            Commands.RegisterCommands<RequestedCommands>();
            Commands.RegisterCommands<FrozenCommands>();
            Commands.RegisterCommands<WarnCommands>();

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            BotClient.MessageCreated += FilterSystem.BotClient_MessageCreated;
            BotClient.MessageCreated += FrozenCommands.BotClient_MessageCreated;
            BotClient.MessageCreated += WarnCommands.BotClient_MessageCreated;
            BotClient.MessageCreated += FunStuff.BotClient_MessageCreated;

            BotClient.MessageReactionAdded += Rimboard.BotClient_MessageReactionAdded;

            BotClient.MessageUpdated += FilterSystem.BotClient_MessageUpdated;
            BotClient.MessageUpdated += WarnCommands.BotClient_MessageUpdated;

            BotClient.ClientErrored += BotClient_ClientErrored;

            BotClient.Heartbeated += ReminderSystem.BotClient_Heartbeated;
            BotClient.Heartbeated += FunStuff.BotClient_Heartbeated;

            BotClient.Ready += BotClient_Ready;

            FilterSystem.FilterTriggered += BotClient_FilterTriggered;

            await BotClient.ConnectAsync();
            await Task.Delay(-1);
        }

        public static async Task SendWebhookMessage(string content = null, DiscordEmbed[] embeds = null, FileStream fileStream = null, string fileName = null)
        {
            var dwb = new DiscordWebhookBuilder();

            if (!(embeds is null))
            {
                if(embeds.Length > 10)
                {
                    throw new ArgumentException("More than 10 embeds provided.");
                }

                dwb.AddEmbeds(embeds);
            }

            if(!(content is null))
            {
                dwb.WithContent(content);
            }

            if (!(fileStream is null) && !(fileName is null))
            {
                dwb.AddFile(fileName, fileStream);
            }

            if (embeds is null && content is null && fileStream is null)
            {
                throw new ArgumentException("Cannot send an empty message.");
            }


            await Webhook.ExecuteAsync(dwb);
        }

        /// <summary>Send a message to the filter channel.</summary>
        /// <param name="embed">The embed to send</param>
        /// <param name="text">The text to send</param>
        /// <returns></returns>
        public static async Task NotifyFilterChannel(DiscordEmbed embed, string text = @"")
        {
            var auditChannel = await BotClient.GetChannelAsync(BotSettings.FilterChannelId);

            try
            {
                await auditChannel.SendMessageAsync
                    (
                        content: text == String.Empty ? null : text,
                        embed: embed
                    ).ConfigureAwait(false);
            } catch 
            {
                Console.WriteLine("Audit channel not found.");
            }
        }

        /// <summary>Sends a message to the action logs channel.</summary>
        public static async Task NotifyActionChannel(string text)
        {
            var actionChannel = await BotClient.GetChannelAsync(BotSettings.ActionChannelId);

            if (text.Length > 0 && actionChannel.Id > 0)
            {
                try
                {
                    await actionChannel.SendMessageAsync(text).ConfigureAwait(false);
                }
                catch
                {
                    Console.WriteLine("Action channel not found.");
                }
            }
        }

        /// <summary>Notify someone they do not have permissions to use a command.</summary>
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

        /// <summary>Load config files.</summary>
        private string LoadConfig(out WebhookInfo info)
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


            Console.Write("\tWebhook");
            using (var fs = File.OpenRead(@"webhook"))
            {
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    string contents = sr.ReadToEnd();

                    info = JsonConvert.DeserializeObject<WebhookInfo>(contents);
                }
            }

            if(!info.Equals(default(WebhookInfo)))
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

        /// <summary>Bot is loaded.</summary>
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

        /// <summary>Someone fucked up with a command.</summary>
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

        /// <summary>A command was executed.</summary>
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
        {
            try
            {
                await FilterSystem.HandleFilterTriggered(e);
            } catch { }
        }
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
