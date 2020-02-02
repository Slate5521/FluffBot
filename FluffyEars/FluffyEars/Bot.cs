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
                EnableDefaultHelp = false,
                EnableMentionPrefix = true,
                EnableDms = false
            };

            Commands = BotClient.UseCommandsNext(commandConfig);

            Commands.RegisterCommands<Commands.ConfigCommands>();
            Commands.RegisterCommands<Commands.BadWordCommands>();
            Commands.RegisterCommands<Commands.ReminderCommands>();

            BotClient.MessageCreated += BotClient_MessageCreated;
            BotClient.MessageUpdated += BotClient_MessageUpdated;
            BotClient.ClientErrored += BotClient_ClientErrored;
            BotClient.Heartbeated += BotClient_Heartbeated;

            await BotClient.ConnectAsync();
            await Task.Delay(-1);
        }

        /// <summary>Called when the bot is heartbeated.</summary>
        private async Task BotClient_Heartbeated(HeartbeatEventArgs e)
        {
            Reminder curReminder;                   // The reminder currently being inspected within the loop.
            Reminder prevReminder = new Reminder(); // The reminder from the previous step of the loop. Defaults to a default Reminder object
                                                    // because a comparison will be done between curReminder and prevReminder, and theoretically
                                                    // there should never be a Reminder object stored in memory that is the default object.

            // Check if there's any notifications. If there are, set the curReminder to the most present reminder before entering the loop.
            if (ReminderSystem.HasNotification())
                curReminder = ReminderSystem.GetSoonestNotification();
            else return; // Note to self: I hate using return in the middle of a method due to optimization reasons. If I can change this later,
                         // I would love that.

            // Bit confusing what I'm doing here. So, basically...
            // 
            // Every time a reminder has notified, it is immediately popped from the list. Therefore, if a reminder from the previous 
            // step is equal to the reminder in the current step, that means the soonest reminder hasn't actually been notified. Therefore,
            // anything later doesn't need to be checked. On the contrary, if a reminder from the previous step is not equal to the 
            // reminder in the current step, that means we should continually check until we find a reminder that isn't ready to be
            // notified.
            while (ReminderSystem.HasNotification() && !curReminder.Equals(prevReminder))
            {
                DateTimeOffset reminderTime = DateTimeOffset.FromUnixTimeMilliseconds(curReminder.Time);

                DateTimeOffset utcNow = DateTimeOffset.UtcNow;

                // If the Reminder's time is NOW or PAST, enter this scope.
                if (utcNow.Ticks >= reminderTime.Ticks)
                {
                    // How late the notification is. Necessary for two reasons:
                    // 1 - The bot may, for unknown reasons in the future, have some sort of extended downtime, so we should keep track of lateness.
                    // 2 - Reminders are only checked every bot heartbeat, so a notification can be anywhere from 40-60 seconds late.
                    TimeSpan lateBy = utcNow.Subtract(reminderTime); 

                    // DEB!
                    DiscordEmbedBuilder deb = new DiscordEmbedBuilder();
                    deb.WithTitle("Notification");
                    deb.WithDescription(curReminder.Text);
                    deb.AddField("Late by", lateBy.ToString());

                    await BotClient.SendMessageAsync(
                        channel: await BotClient.GetChannelAsync(BotSettings.AuditChannelId),
                        content: String.Format("<@{0}>", curReminder.User),
                        tts: false,
                        embed: deb);

                    // Remove the reminder from the list.
                    ReminderSystem.RemoveReminder(curReminder);
                }

                // The current reminder will become the next step's previous reminder, and we should also grab the next reminder before heading into
                // the next step.
                prevReminder = curReminder;
                curReminder = ReminderSystem.GetSoonestNotification();
            }
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
            if(BadwordSystem.HasBadWord(message.Content) && BotSettings.AuditChannelId != 0)
            {
                string badWord = BadwordSystem.GetBadWord(message.Content); // The detected bad word.

                // DEB!
                DiscordEmbedBuilder deb = new DiscordEmbedBuilder();

                deb.WithTitle("Bad word detected");
                deb.WithColor(DiscordColor.Cyan);
                deb.WithDescription(String.Format("{0} may have used a bad word: {1} in the channel\n{2}", message.Author.Mention, badWord, 
                    String.Format("https://discordapp.com/channels/{0}/{1}/{2}", message.Channel.GuildId, message.ChannelId, message.Id)));
                
                // Grab the audit channel.
                DiscordChannel auditChannel = await BotClient.GetChannelAsync(BotSettings.AuditChannelId);

                await auditChannel.SendMessageAsync(embed: deb).ConfigureAwait(false);
            }
        }
        
    }
}
