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
            public ulong FilterChannelId;
            /// <summary>A list of channels that will not be checked.</summary>
            public List<ulong> ExcludedChannels;
            /// <summary>The max length of a message before it's considered spam.</summary>
            public int MessageMaxLength;
            /// <summary>The max amount of linesplits of a message before it's considered spam.</summary>
            public int MessageMaxSplits;
            /// <summary>The max amount of messages in a second before it's considered spam.</summary>
            public int MaxMessagesPerSecond;
            /// <summary>The amount of time to wait before being triggered by a user's spam</summary>
            public int SpamTimeout;
        }

        /// <summary>Default bot settings.</summary>
        static botSettings_ DefaultBotSettings = new botSettings_
        {
            FilterChannelId = 674884683166646283,
            ExcludedChannels = new List<ulong>(),
            MessageMaxLength = 800,
            MessageMaxSplits = 5,
            MaxMessagesPerSecond = 3,
            SpamTimeout = 30000 // 30 seconds by default.
        };

        /// <summary>Bot settings!</summary>
        private static botSettings_ botSettings;
        
        public static ulong FilterChannelId
        {
            get => botSettings.FilterChannelId;
            set => botSettings.FilterChannelId = value;
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
        #region Spam
        public static int MaxMessageLength
        {
            get => botSettings.MessageMaxLength;
        }
        public static int MaxMessageSplits
        {
            get => botSettings.MessageMaxSplits;
        }
        public static int MaxMessagesPerSecond
        {
            get => botSettings.MaxMessagesPerSecond;
        }
        public static int SpamTimeout
        {
            get => botSettings.SpamTimeout;
        }
        #endregion
    }
}