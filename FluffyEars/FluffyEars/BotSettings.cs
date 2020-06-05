﻿// BotSettings.cs
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
            /// <summary>Whether or not the bot should announce it has started.</summary>
            public bool StartMessageEnabled;

            public List<string> FrozenNames;
        }

        /// <summary>Default bot settings.</summary>
        static botSettings_ DefaultBotSettings = new botSettings_
        {
            FilterChannelId = 674884683166646283,
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
        
        public static ulong FilterChannelId
        {
            get => botSettings.FilterChannelId;
            set => botSettings.FilterChannelId = value;
        }

        public static bool StartMessageEnabled
        {
            get => botSettings.StartMessageEnabled;
            set => botSettings.StartMessageEnabled = value;
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

        public static void AddFrozenName(string name)
        {
            if (!IsFrozenName(name))
                botSettings.FrozenNames.Add(name);
        }

        public static void RemoveFrozenName(string name)
        {
            if (IsFrozenName(name))
                botSettings.FrozenNames.Remove(name);
        }

        public static bool IsFrozenName(string name)
            => botSettings.FrozenNames.Contains(name);

        public static string[] GetFrozenNames()
            => botSettings.FrozenNames.ToArray();

        #region Channel exclusion

        /// <summary>Exclude the channel from bad word detection.</summary>
        public static void ExcludeChannel(DiscordChannel chan)
        {
            if(!botSettings.ExcludedChannels.Contains(chan.Id))
                botSettings.ExcludedChannels.Add(chan.Id);
        }

        /// <summary>Un-exclude the channel from bad word detection.</summary>
        public static void IncludeChannel(DiscordChannel chan)
        {
            if (botSettings.ExcludedChannels.Contains(chan.Id))
                botSettings.ExcludedChannels.Remove(chan.Id);
        }

        public static int GetExcludedChannelsCount => botSettings.ExcludedChannels.Count;

        /// <summary>Get excluded channels.</summary>
        public static async Task<DiscordChannel[]> GetExcludedChannels()
        {
            DiscordChannel[] channels;

            if (botSettings.ExcludedChannels.Count > 0)
            {
                channels = new DiscordChannel[botSettings.ExcludedChannels.Count];

                for(int i = 0; i < botSettings.ExcludedChannels.Count; i++)
                    channels[i] = await Bot.BotClient.GetChannelAsync(botSettings.ExcludedChannels[i]);
            }
            else
                channels = Array.Empty<DiscordChannel>();

            return channels;
        }
        /// <summary>Check if the channel is excluded.</summary>
        /// <returns>True if the channel is excluded.</returns>
        public static bool IsChannelExcluded(DiscordChannel chan) => botSettings.ExcludedChannels.Contains(chan.Id);

        #endregion Channel exclusion
    }
}