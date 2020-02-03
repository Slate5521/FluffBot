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

namespace FluffyEars
{
    class Bot
    {
        /// <summary>The bot client.</summary>
        public static DiscordClient BotClient;
        /// <summary>CommandNextModule</summary>
        public CommandsNextModule Commands;

        /// <summary>Start this shit up!</summary>
        public async Task RunAsync()
        {
            string authKey = String.Empty;

            // --------------------
            // Load shit.

            // Authkey
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
                Console.WriteLine(@"... Loaded.");

            // Bot settings

           Console.Write("\tSettings");
            
            if(BotSettings.CanLoad())
            {
                BotSettings.Load();
                Console.WriteLine("... Loaded!");
            } else
            {
                BotSettings.Default();
                Console.WriteLine("... Not found. Loading default values.");
            }

            Console.Write("\tReminders");

            if (ReminderSystem.CanLoad())
            {
                ReminderSystem.Load();
                Console.WriteLine("... Loaded!");
            }
            else
            {
                ReminderSystem.Default();
                Console.WriteLine("... Not found. Instantiating default values.");
            }

            Console.Write("\tBadwords");

            if (BadwordSystem.CanLoad())
            {
                BadwordSystem.Load();
                Console.WriteLine("... Loaded!");
            }
            else
            {
                BadwordSystem.Default();
                Console.WriteLine("... Not found. Instantiating default values.");
            }


            DiscordConfiguration botConfig = new DiscordConfiguration
            {
                Token = authKey,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Critical | LogLevel.Debug | LogLevel.Error | LogLevel.Info | LogLevel.Warning,
            };

            BotClient = new DiscordClient(botConfig);

            CommandsNextConfiguration commandConfig = new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false,
                EnableDms = true
            };

            Commands = BotClient.UseCommandsNext(commandConfig);

            Commands.RegisterCommands<Commands.ConfigCommands>();
            Commands.RegisterCommands<Commands.BadWordCommands>();
            Commands.RegisterCommands<Commands.ReminderCommands>();
            Commands.RegisterCommands<Commands.HelpCommand>();

            BotClient.MessageCreated += BotClient_MessageCreated;
            BotClient.MessageUpdated += BotClient_MessageUpdated;
            BotClient.ClientErrored += BotClient_ClientErrored;
            BotClient.Heartbeated += ReminderSystem.BotClient_Heartbeated;

            await BotClient.ConnectAsync();
            await Task.Delay(-1);
        }

        /// <summary>Some shit broke.</summary>
        private Task BotClient_ClientErrored(ClientErrorEventArgs e)
        {
            Console.WriteLine(e.EventName);
            Console.WriteLine(e.Exception);
            Console.WriteLine(e.Exception.InnerException);

            return Task.CompletedTask;
        }

        /// <summary>A text message was updated by a user.</summary>
        private async Task BotClient_MessageUpdated(MessageUpdateEventArgs e)
        {
            // If this channel is excluded, let's not even bother checking it.
            if (!BotSettings.IsChannelExcluded(e.Channel))
                await CheckMessage(e.Message);
        }

        /// <summary>A text message was sent to the Guild by a user</summary>
        private async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            // If this channel is excluded, let's not even bother checking it.
            if (!BotSettings.IsChannelExcluded(e.Channel))
                await CheckMessage(e.Message);
        }

        /// <summary>Check the messages for any Bad Words aka slurs.</summary>
        /// <param name="message">The message object to inspect.</param>
        private async Task CheckMessage(DiscordMessage message)
        {
            // Let's check (a) does this message have a bad word? && (b) is this channel NOT the audit channel?
            if(BadwordSystem.HasBadWord(message.Content) && BotSettings.FilterChannelId != 0)
            {
                string badWord = BadwordSystem.GetBadWord(message.Content); // The detected bad word.

                // DEB!
                DiscordEmbedBuilder deb = new DiscordEmbedBuilder();

                deb.WithTitle("Bad word detected");
                deb.WithColor(DiscordColor.Cyan);
                deb.WithDescription(String.Format("{0} may have used a bad word: {1} in the channel\n{2}", message.Author.Mention, badWord, 
                    String.Format("https://discordapp.com/channels/{0}/{1}/{2}", message.Channel.GuildId, message.ChannelId, message.Id)));
                
                // Grab the audit channel.
                DiscordChannel auditChannel = await BotClient.GetChannelAsync(BotSettings.FilterChannelId);

                await auditChannel.SendMessageAsync(embed: deb).ConfigureAwait(false);
            }
        }
        
    }
}
