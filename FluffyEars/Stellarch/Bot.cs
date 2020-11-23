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
using System.Collections.Generic;
using DSharpPlus.CommandsNext.Attributes;

namespace BigSister
{
    public static class Bot
    {
        static Timer reminderTimer;
        static ulong[] channelIds = new ulong[]         { 755926052571971814 };
        static string[] mentionStrings = new string[] { "<@&727048568220680252>" };
        static DiscordChannel[] channels;

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
            botClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                EnableDms = false,
                StringPrefixes = new string[] { "!" },
                CaseSensitive = false,
                DmHelp = false,
                EnableDefaultHelp = false
            });
        }

        class Commands : BaseCommandModule
        {
            [Command("dropped")]
            static async Task DroppedPotato(string twitchName)
            {

            }
        }

        static void RegisterEvents(DiscordClient botClient)
        {
            // ----------------
            // Reminder Timer

            reminderTimer.Elapsed += ReminderTimer_Elapsed;

            // Event handle reminder timer here.
        }

        private static void ReminderTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check if there are any pending.
            if(Announcements.Instance.IsPending())
            {   // There are some announcements pending, so let's get which ones they are.
                List<AnnouncementInfo> announcementsPost =
                    Announcements.Instance.GetPendingAnnouncements();

                foreach(AnnouncementInfo info in announcementsPost)
                {
                    // Check if the announcement is too late.
                    if (DateTimeOffset.UtcNow <= info.EndTime)
                    {   // Not too late.
                        PostAnnouncement(info).ConfigureAwait(false).GetAwaiter().GetResult();
                        Announcements.Instance.PopAnnouncement(info);
                    }
                    else
                    {   // Too late, so let's jus tremove it.
                        Announcements.Instance.PopAnnouncement(info);
                    }
                }

            }
        }

        static async Task PostAnnouncement(AnnouncementInfo info)
        {
            // Check if the channel cache is null.
            if(channels is null)
            {   // It is, so let's just populate it really fast.
                channels = new DiscordChannel[channelIds.Length];

                for(int i = 0; i < channels.Length; i++)
                {   // Let's populate each one.
                    channels[i] = await Program.BotClient.GetChannelAsync(channelIds[i]);
                }
            }

            // Go through each channel to post an announcement...
            for(int i = 0; i < channels.Length; i++) 
            {
                await channels[i].SendMessageAsync(content: info.GetContent(mentionStrings[i]));//, 
                    //embed: info.GetAnnouncementEmbed());
                await Task.Delay(2000);
            }
        }
    }
}
