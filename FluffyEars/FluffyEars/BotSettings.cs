// BotSettings.cs
// All of the bot settings are stored here.

using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace FluffyEars
{
    /// <summary>This is the static class that houses all the bot's settings. </summary>
    public static class BotSettings
    {
        /// <summary>The file to save Exclude information to.</summary>
        private const string BaseFile = "settings";
        /// <summary>The SaveFile object for this class.</summary>
        /// <see cref="SaveFile"/>
        private static SaveFile saveFile = new SaveFile(BaseFile);
        /// <summary>The lock object for this class' I/O operations.</summary>
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

        /// <summary>A JSON serializable struct that contains the actual settings.</summary>
        private struct botSettings_
        {
            /// <summary>Filter log channel.</summary>
            public ulong FilterChannelId;
            /// <summary>ActionLog channel.</summary>
            public ulong ActionChannelId;
            /// <summary>Rimboard channel.</summary>
            public ulong RimboardChannelId;
            /// <summary>How long to search for warnings, in months.</summary>
            public int WarnThresholdMonths;
            /// <summary>A list of channels that will not be checked.</summary>
            public List<ulong> ExcludedChannels;
            /// <summary>Whether or not the bot should announce it has started.</summary>
            public bool StartMessageEnabled;
            /// <summary>Whether or not the bot should start snooping when it sees a warn.</summary>
            public bool AutoWarnSnoopEnabled;
            /// <summary>Whether or not Rimboard should be enabled.</summary>
            public bool RimboardEnabled;
            /// <summary>Whether or not FunStuff should be enabled.</summary>
            public bool FunEnabled;
            /// <summary>The Rimboard emoji.</summary>
            public string RimboardEmoji;
            /// <summary>The number of reacts to pin a message.</summary>
            public int RimboardThreshold;
        }

        /// <summary>Default bot settings.</summary>
        private static botSettings_ DefaultBotSettings = new botSettings_
        {
            FilterChannelId = 724187658158342204, // FDS #donteverdelete
            ActionChannelId = 724187658158342204, // FDS #donteverdelete
            RimboardChannelId = 724187658158342204, // FDS #donteverdelete
            WarnThresholdMonths = 6, // 6 months
            ExcludedChannels = new List<ulong>(),
            StartMessageEnabled = false,
            RimboardEnabled = false,
            AutoWarnSnoopEnabled = false,
            FunEnabled = false,
            RimboardEmoji = ":rimworld:",
            RimboardThreshold = 5
        };

        /// <summary>Bot settings!</summary>
        private static botSettings_ botSettings;

        #region Public Fields

        /// <summary>The Filter Channel's Id</summary>
        public static ulong FilterChannelId
        {
            get => botSettings.FilterChannelId;
            set => botSettings.FilterChannelId = value;
        }

        public static ulong ActionChannelId
        {
            get => botSettings.ActionChannelId;
            set => botSettings.ActionChannelId = value;
        }
        public static ulong RimboardChannelId
        {
            get => botSettings.RimboardChannelId;
            set => botSettings.RimboardChannelId = value;
        }
        public static int WarnThreshold
        {
            get => botSettings.WarnThresholdMonths;
            set => botSettings.WarnThresholdMonths = value;
        }

        /// <summary>If start message should be enabled.</summary>
        public static bool StartMessageEnabled
        {
            get => botSettings.StartMessageEnabled;
            set => botSettings.StartMessageEnabled = value;
        }

        /// <summary>Whether or not the bot should start snooping when it sees a warn.</summary>
        public static bool AutoWarnSnoopEnabled
        {
            get => botSettings.AutoWarnSnoopEnabled;
            set => botSettings.AutoWarnSnoopEnabled = value;
        }

        /// <summary>Whether or not Rimboard should be enabled.</summary>
        public static bool RimboardEnabled
        {
            get => botSettings.RimboardEnabled;
            set => botSettings.RimboardEnabled = value;
        }


        /// <summary>Whether or not fun should be allowed.</summary>
        public static bool FunEnabled
        {
            get => botSettings.FunEnabled;
            set => botSettings.FunEnabled = value;
        }

        public static string RimboardEmoji
        {
            get => botSettings.RimboardEmoji;
            set => botSettings.RimboardEmoji = value;
        }

        public static int RimboardThreshold
        {
            get => botSettings.RimboardThreshold;
            set => botSettings.RimboardThreshold = value;
        }

        #endregion Public Fields
        // ################################
        #region Save/Load Methods

        /// <summary>Instantiate default values for this class.</summary>
        public static void Default()
        {
            botSettings = DefaultBotSettings;

            Save();
        }

        /// <summary>Save the class to its save file.</summary>
        public static void Save() 
            => saveFile.Save(botSettings, lockObj);

        /// <summary>Checks if the expected save file for this class can be loaded from.</summary>
        public static bool CanLoad() 
            => saveFile.IsExistingSaveFile();

        /// <summary>Loads the save file or instantiates default values if unable to load.</summary>
        public static void Load()
        {
            if (CanLoad())
            {
                botSettings = saveFile.Load<botSettings_>(lockObj);
            }
            else
            {
                Default();
            }
        }

        #endregion Save/Load Methods
        // ################################
        #region Channel Exclusion

        /// <summary>Exclude the channel from bad word detection.</summary>
        public static void ExcludeChannel(DiscordChannel chan)
        {
            if (!botSettings.ExcludedChannels.Contains(chan.Id))
            {   // The specified channel is not an excluded channel.
                botSettings.ExcludedChannels.Add(chan.Id);
            }
        }

        /// <summary>Un-exclude the channel from bad word detection.</summary>
        public static void IncludeChannel(DiscordChannel chan)
        {
            if (botSettings.ExcludedChannels.Contains(chan.Id))
            {   // The specified channel is an excluded channel.
                botSettings.ExcludedChannels.Remove(chan.Id);
            }
        }

        /// <summary>Get the number of excluded channels.</summary>
        public static int GetExcludedChannelsCount 
            => botSettings.ExcludedChannels.Count;

        /// <summary>Get excluded channels.</summary>
        public static async Task<DiscordChannel[]> GetExcludedChannels()
        {
            List<DiscordChannel> channels = new List<DiscordChannel>();

            if (botSettings.ExcludedChannels.Count > 0)
            {   // There is at least one excluded channel.
                for (int i = 0; i < botSettings.ExcludedChannels.Count; i++)
                {
                    try
                    {
                        channels.Add(await Bot.BotClient.GetChannelAsync(botSettings.ExcludedChannels[i]));
                    } catch { }
                }
            }
            else
            {   // There are no excluded channels.
            }

            return channels.ToArray();
        }
        /// <summary>Check if the channel is excluded.</summary>
        /// <returns>True if the channel is excluded.</returns>
        public static bool IsChannelExcluded(DiscordChannel chan) 
            => botSettings.ExcludedChannels.Contains(chan.Id);

        #endregion Channel exclusion
    }
}