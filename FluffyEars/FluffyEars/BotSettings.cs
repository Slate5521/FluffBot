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
            /// <summary>Audit log channel.</summary>
            public ulong FilterChannelId;
            /// <summary>A list of channels that will not be checked.</summary>
            public List<ulong> ExcludedChannels;
            /// <summary>Whether or not the bot should announce it has started.</summary>
            public bool StartMessageEnabled;
            /// <summary>A list of recognized Frozen names.</summary>
            public List<string> FrozenNames;
        }

        /// <summary>Default bot settings.</summary>
        private static botSettings_ DefaultBotSettings = new botSettings_
        {
            FilterChannelId = 674884683166646283, // #filter-logs
            ExcludedChannels = new List<ulong>(),
            StartMessageEnabled = false,
            FrozenNames = new List<string>() 
            { 
                "frozen",
                "frozo",
                "forza",
                "freezy"
            }
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

        /// <summary>If start message should be enabled.</summary>
        public static bool StartMessageEnabled
        {
            get => botSettings.StartMessageEnabled;
            set => botSettings.StartMessageEnabled = value;
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
        #region Frozen Methods

        /// <summary>Add a recognized Frozen moniker.</summary>
        public static void AddFrozenName(string name)
        {
            if (!IsFrozenName(name))
            {   // This is not a frozen name.
                botSettings.FrozenNames.Add(name);
            }
        }

        /// <summary>Remove a name from the list of recognized Frozen monikers.</summary>
        public static void RemoveFrozenName(string name)
        {
            if (IsFrozenName(name))
            {   // This is a frozen name.
                botSettings.FrozenNames.Remove(name);
            }
        }

        /// <summaryCheck if a specified name is a recognized Frozen moniker.</summary>
        public static bool IsFrozenName(string name)
            => botSettings.FrozenNames.Contains(name);

        /// <summary>Get all the recognized Frozen monikers.</summary>
        public static string[] GetFrozenNames()
            => botSettings.FrozenNames.ToArray();

        #endregion Frozen Methods
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
            DiscordChannel[] channels;

            if (botSettings.ExcludedChannels.Count > 0)
            {   // There is at least one excluded channel.
                channels = new DiscordChannel[botSettings.ExcludedChannels.Count];

                for (int i = 0; i < botSettings.ExcludedChannels.Count; i++)
                {
                    channels[i] = await Bot.BotClient.GetChannelAsync(botSettings.ExcludedChannels[i]);
                }
            }
            else
            {   // There are no excluded channels.
                channels = Array.Empty<DiscordChannel>();
            }

            return channels;
        }
        /// <summary>Check if the channel is excluded.</summary>
        /// <returns>True if the channel is excluded.</returns>
        public static bool IsChannelExcluded(DiscordChannel chan) 
            => botSettings.ExcludedChannels.Contains(chan.Id);

        #endregion Channel exclusion
    }
}