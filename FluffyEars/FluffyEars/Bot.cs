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
using System.Collections.Generic;
using System.Linq;
using FluffyEars.Spam;

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

            if (FilterSystem.CanLoad())
            {
                FilterSystem.Load();
                Console.WriteLine("... Loaded!");
            }
            else
            {
                FilterSystem.Default();
                Console.WriteLine("... Not found. Instantiating default values.");
            }

            Console.Write("\tExcludes");

            if (Excludes.CanLoad())
            {
                Excludes.Load();
                Console.WriteLine("... Loaded!");
            }
            else
            {
                Excludes.Default();
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
            Commands.RegisterCommands<Commands.FilterCommands>();
            Commands.RegisterCommands<Commands.ReminderCommands>();
            Commands.RegisterCommands<Commands.HelpCommand>();
            Commands.RegisterCommands<Commands.SpamCommands>();

            BotClient.MessageCreated += BotClient_MessageCreated;
            BotClient.MessageCreated += SpamFilter.BotClient_MessageCreated;

            BotClient.MessageUpdated += BotClient_MessageUpdated;
            BotClient.ClientErrored += BotClient_ClientErrored;
            BotClient.Heartbeated += ReminderSystem.BotClient_Heartbeated;

            Spam.SpamFilter.SpamDetected += BotClient_SpamDetected;

            await BotClient.ConnectAsync();
            await Task.Delay(-1);
        }

        private async void BotClient_SpamDetected(SpamEventArgs e)
        {
            DiscordChannel auditChannel = await BotClient.GetChannelAsync(BotSettings.FilterChannelId);

            await auditChannel.SendMessageAsync("SPAM!!!!!!! (placeholder message for now)").ConfigureAwait(false);

            
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
            // Skip if (1) this channel is excluded or (2) this is sent by the bot.
            if (!BotSettings.IsChannelExcluded(e.Channel) && !e.Author.IsBot)
                await CheckMessage(e.Message);
        }
        private readonly string[] frozenWords = { "frozen", "frozo", "forza", "freezy" };
        /// <summary>A text message was sent to the Guild by a user</summary>
        private async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            // Frozen told me to add this!!!
            
            if(frozenWords.Any(e.Message.Content.ToLower().Contains))
            {
                DiscordChannel chan = await BotClient.GetChannelAsync(679933620034600960);
                await chan.SendMessageAsync(String.Format("<@113829933071073287> Someone mentioned you!\n{0}",
                    String.Format("https://discordapp.com/channels/{0}/{1}/{2}", e.Channel.GuildId, e.Channel.Id, e.Message.Id)));
            }


            // Skip if (1) this channel is excluded or (2) this is sent by the bot.
            if (!BotSettings.IsChannelExcluded(e.Channel) && !e.Author.IsBot)
                await CheckMessage(e.Message);
        }

        /// <summary>Check the messages for any Bad Words aka slurs.</summary>
        /// <param name="message">The message object to inspect.</param>
        private async Task CheckMessage(DiscordMessage message)
        {
            // Let's check if the audit channel is set.
            if(BotSettings.FilterChannelId != 0)
            {
                List<string> badWords = FilterSystem.GetBadWords(message.Content); // The detected bad words.

                if (badWords.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach(string word in badWords)
                        sb.Append(word + ' ');

                    // DEB!
                    DiscordEmbedBuilder deb = new DiscordEmbedBuilder();

                    deb.WithTitle("Filter: Word Detected");
                    deb.WithColor(DiscordColor.Cyan);
                    deb.WithDescription(String.Format("{0} has triggered the filter system. Words possibly detected:\n{1} in the channel\n{2}",
                        /*{0}*/ message.Author.Mention,
                        /*{1}*/ sb.ToString(),
                        /*{2}*/ String.Format("https://discordapp.com/channels/{0}/{1}/{2}", message.Channel.GuildId, message.ChannelId, message.Id)));

                    // Grab the audit channel.
                    DiscordChannel auditChannel = await BotClient.GetChannelAsync(BotSettings.FilterChannelId);

                    await auditChannel.SendMessageAsync(embed: deb).ConfigureAwait(false);
                }
            }
        }
        
    }
}
