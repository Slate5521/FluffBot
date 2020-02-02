// BotSettings.cs
// All of the bot settings are stored here.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using FluffyEars.Reminders;

namespace FluffyEars
{
    /// <summary>This is the static class that houses all the bot's settings. </summary>
    public static class BotSettings
    {
        private static SaveFile saveFile = new SaveFile(BaseFile);
        /// <summary>A JSON serializable struct that contains the actual settings.</summary>
        private struct botSettings_
        {
            /// <summary>Audit log channel.</summary>
            public ulong AuditChannelId;
            /// <summary>Notification channel (for Reminders).</summary>
            public ulong NotifChannelId;
            /// <summary>A list of users who can use bot config features.</summary>
            public List<ulong> WhitelistedUsers;
            /// <summary>A list of channels where bad words will not be checked for.</summary>
            public List<ulong> ExcludedChannels;
        }

        /// <summary>Default bot settings.</summary>
        static botSettings_ DefaultBotSettings = new botSettings_
        {
            AuditChannelId = 0,
            NotifChannelId = 0,
            WhitelistedUsers = new List<ulong>() { 131626628211146752 },
            ExcludedChannels = new List<ulong>(),
        };

        /// <summary>Bot settings!</summary>
        private static botSettings_ botSettings;

        /// <summary>Notification channel for bot settings.</summary>
        public static ulong NotificationChannelId
        {
            get => botSettings.NotifChannelId;
            set => botSettings.NotifChannelId = value;
        }

        public static ulong AuditChannelId
        {
            get => botSettings.AuditChannelId;
            set => botSettings.AuditChannelId = value;
        }

        // I will not be documenting these for the moment.
        #region Initialization, deconstruction commands

        public const string BaseFile = "settings";
        private static readonly object lockObj = (object)
    @"
         ,
        /|      __
       / |   ,-~ /
      Y :|  //  /
      | jj /( .^
      >-'~'-v'
     /       Y
    jo  o    |
   ( ~T~j
    >._-' _./
   /   '~'  |
  Y _,  |
 /| ;-'~ _  l
/ l/ ,-'~    \
\//\/      .- \
 Y        /    Y    -Row
 l       I     !
 ]\      _\    /'\
(' ~----( ~   Y.  )";
///////////////////////

        public static void Default()
        {
            botSettings = DefaultBotSettings;
            Save();
        }

        public static void Save() => saveFile.Save<botSettings_>(botSettings, lockObj);

        public static bool CanLoad() => saveFile.IsExistingSaveFile();

        public static void Load()
        {
            if (CanLoad())
                botSettings = saveFile.Load<botSettings_>(lockObj);
            else Default();
        }

        #endregion Initialization, deconstruction commands
        
        #region Whitelist

        /// <summary>Check if a user can use configuration commands.</summary>
        /// <returns>True if the user can use configuration commands.</returns>
        public static bool CanUseConfigCommands(DiscordMember user)
        {
            return user.IsOwner || botSettings.WhitelistedUsers.Contains(user.Id);
        }
        /// <summary>Add a user to the white list for configuration commands.</summary>
        public static void AddUserToWhitelist(DiscordMember user)
        {
            if(!botSettings.WhitelistedUsers.Contains(user.Id))
                botSettings.WhitelistedUsers.Add(user.Id);
        }

        /// <summary>Remove a user from the white list for configuration commands.</summary>
        public static void RemoveUserFromWhitelist(DiscordMember user)
        {
            if (botSettings.WhitelistedUsers.Contains(user.Id))
                botSettings.WhitelistedUsers.Remove(user.Id);
        }

        /// <summary>Get users in the whitelist.</summary>
        /// <returns>A ulong array representing the IDs of every user in the list.</returns>
        public static ulong[] GetWhitelist()
        {
            return botSettings.WhitelistedUsers.ToArray();
        }

        /// <summary>Check if the user is whitelisted.</summary>
        /// <returns>True if the user is whitelisted.</returns>
        public static bool IsUserOnWhitelist(DiscordMember user) => botSettings.WhitelistedUsers.Contains(user.Id);
        
        #endregion Whitelist
        #region Channel exclusion

        /// <summary>Exclude the channel from bad word detection.</summary>
        public static void ExcludeChannel(DiscordChannel chan)
        {
            if(!DefaultBotSettings.ExcludedChannels.Contains(chan.Id))
                DefaultBotSettings.ExcludedChannels.Add(chan.Id);
        }

        /// <summary>Un-exclude the channel from bad word detection.</summary>
        public static void IncludeChannel(DiscordChannel chan)
        {
            if (DefaultBotSettings.ExcludedChannels.Contains(chan.Id))
                DefaultBotSettings.ExcludedChannels.Remove(chan.Id);
        }

        /// <summary>Get excluded channels.</summary>
        public static async Task<DiscordChannel[]> GetExcludedChannels()
        {
            DiscordChannel[] channels;

            if (DefaultBotSettings.ExcludedChannels.Count > 0)
            {
                channels = new DiscordChannel[DefaultBotSettings.ExcludedChannels.Count];

                for(int i = 0; i < DefaultBotSettings.ExcludedChannels.Count; i++)
                    channels[i] = await Bot.BotClient.GetChannelAsync(DefaultBotSettings.ExcludedChannels[i]);
            }
            else
                channels = Array.Empty<DiscordChannel>();

            return channels;
        }
        /// <summary>Check if the channel is excluded.</summary>
        /// <returns>True if the channel is excluded.</returns>
        public static bool IsChannelExcluded(DiscordChannel chan) => DefaultBotSettings.ExcludedChannels.Contains(chan.Id);

        #endregion Channel exclusion
    }
}