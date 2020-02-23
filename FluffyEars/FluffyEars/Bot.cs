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

            //BotClient.MessageCreated += BotClient_MessageCreated;FROZENZEZE

            BotClient.MessageCreated += FilterSystem.BotClient_MessageCreated;
            BotClient.MessageCreated += FROZEN.BotClient_MessageCreated;

            BotClient.MessageUpdated += FilterSystem.BotClient_MessageUpdated;
            BotClient.ClientErrored += BotClient_ClientErrored;
            BotClient.Heartbeated += ReminderSystem.BotClient_Heartbeated;

            BotClient.Ready += BotClient_Ready;

            FilterSystem.FilterTriggered += BotClient_FilterTriggered;

            await BotClient.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task BotClient_Ready(ReadyEventArgs e)
        {/*
            try
            {
                await BotClient.SendMessageAsync(await BotClient.GetChannelAsync(326892498096095233),
                    ChatObjects.GetNeutralMessage(@"I'm a bunny."));
                await BotClient.SendMessageAsync(await BotClient.GetChannelAsync(214523379766525963),
                    ChatObjects.GetNeutralMessage(@"I'm a bunny."));
            }
            catch { }

            await SelfAudit.LogSomething(BotClient.CurrentUser, "startup", "n/a");*/
        }

        private async void BotClient_FilterTriggered(FilterEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            foreach(string str in e.BadWords)
            {
                sb.Append(str);

                if(!e.BadWords.Last().Equals(str))
                    sb.Append(@", ");
            }

            // DEB!
            DiscordEmbedBuilder deb = new DiscordEmbedBuilder();

            deb.WithTitle("Filter: Word Detected");
            deb.WithColor(DiscordColor.Red);
            deb.WithDescription(String.Format("{0} has triggered the filter system. Words possibly detected:\n{1}\n in the channel\n{2}",
                /*{0}*/ e.User.Mention,
                /*{1}*/ sb.ToString(),
                /*{2}*/ String.Format("https://discordapp.com/channels/{0}/{1}/{2}", e.Channel.GuildId, e.Channel.Id, e.Message.Id)));
            deb.WithThumbnailUrl(ChatObjects.URL_FILTER_BUBBLE);

            await NotifyFilterChannel(deb.Build());
        }

        private static async Task NotifyFilterChannel(DiscordEmbed embed, string text = @"")
        {
            DiscordChannel auditChannel = await BotClient.GetChannelAsync(BotSettings.FilterChannelId);

            await auditChannel.SendMessageAsync(
                content: text == String.Empty ? null : text,
                embed: embed).ConfigureAwait(false);
        }

        /// <summary>Some shit broke.</summary>
        private Task BotClient_ClientErrored(ClientErrorEventArgs e)
        {
            Console.WriteLine(e.EventName);
            Console.WriteLine(e.Exception);
            Console.WriteLine(e.Exception.InnerException);

            return Task.CompletedTask;
        }
    }
}
