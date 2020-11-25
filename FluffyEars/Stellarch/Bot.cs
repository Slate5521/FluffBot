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
using System.Collections.Generic;

namespace BigSister
{
    public static class Bot
    {
        static Timer reminderTimer;                           // Burrow                   , Poe home                 , Rimworld
        static readonly ulong[] channelIds = new ulong[]       {      780596714968186880  ,      701631184832167996  ,      780350282144022548   };
        static readonly string[] mentionStrings = new string[] { @"<@&780596737106378772>", @"<@&701642824487141376>", @"<@&780034969870401566>" };
        static DiscordChannel[] _channels;

        static async Task<DiscordChannel[]> GetChannels()
        {
            if (_channels is null)
            {
                _channels = new DiscordChannel[channelIds.Length];

                for (int i = 0; i < _channels.Length; i++)
                {   // Let's populate each one.
                    _channels[i] = await Program.BotClient.GetChannelAsync(channelIds[i]);
                }
            }

            return _channels;
        }

        public static async Task RunAsync(DiscordClient botClient)
        {
            // Configure timer. Set it up BEFORE registering events.
            reminderTimer = new Timer
            {
                Interval = 60000, // 1 minute
                AutoReset = true
            };

            RegisterCommands(botClient);
            RegisterEvents();

            reminderTimer.Start();

            await botClient.ConnectAsync();
            await Task.Delay(-1);
        }

        static void RegisterCommands(DiscordClient botClient)
        {
            var commands = botClient.GetCommandsNext();

            commands.RegisterCommands<BotCommands>();
        }


        static void RegisterEvents()
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
                        try
                        {
                            PostAnnouncement(info).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        finally
                        {   // Always pop the announcement so it doesn't spam the fuck out of other channels if it can't send it to one.
                            Announcements.Instance.PopAnnouncement(info);
                        }
                    }
                    else
                    {   // Too late, so let's just remove it.
                        Announcements.Instance.PopAnnouncement(info);
                    } // end else
                } // end foreach
            } // end if
        } // end method

        public static async Task PostAnnouncement(AnnouncementInfo info)
        {
            var channels = await GetChannels();

            // Go through each channel to post an announcement...
            for (int i = 0; i < channels.Length; i++)
            {
                await channels[i].SendMessageAsync(content: info.GetContent(mentionStrings[i]));
                await Task.Delay(2000);
            }
        }

        public static async Task PostAnnouncement(string content)
        {
            var channels = await GetChannels();

            // Go through each channel to post an announcement...
            for (int i = 0; i < channels.Length; i++)
            {
                await channels[i].SendMessageAsync(String.Format(content, mentionStrings[i]));
                await Task.Delay(2000);
            }
        }
    }
}
