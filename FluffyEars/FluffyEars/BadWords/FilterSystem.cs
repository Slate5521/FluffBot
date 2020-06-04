﻿// BadwordSystem.cs
// A static class that handles badwords.

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluffyEars.BadWords
{
    public static class FilterSystem
    {
        /// <summary>Invoked when the filter is triggered.</summary>
        public static event FilterTriggeredEventHandler FilterTriggered;
        public delegate void FilterTriggeredEventHandler(FilterEventArgs e);

        private static List<string> filterList;
        public const string BaseFile = "filter";
        private static readonly object lockObj = (object)@"
         \\
          \\_
           (_)
          / )
   jgs  o( )_\_";
        private static SaveFile saveFile = new SaveFile(BaseFile);
        static RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
        private static Regex[] regexSearchList;


        public static void Default()
        {
            filterList = new List<string>();
            regexSearchList = new Regex[0];

            Save();
        }
        public static void Save()
        {
            saveFile.Save<List<string>>(filterList, lockObj);

            UpdatePatternString();
        }
        public static bool CanLoad() => saveFile.IsExistingSaveFile();
        public static void Load()
        {
            if (CanLoad())
            {
                filterList = saveFile.Load<List<string>>(lockObj);
                UpdatePatternString();
            }
            else Default();
        }

        /// <summary>This updates the RegEx megastring, containing all the filter words but in a word1|word2 format.</summary>
        private static void UpdatePatternString()
        {
            //List<string> filterListDesc = filterList.OrderByDescending(a => a.Length).ToList();
            List<string> filterListDesc = filterList;
            filterListDesc.Sort();
            filterListDesc.Reverse();
            
            regexSearchList = new Regex[filterListDesc.Count];

            for (int i = 0; i < filterListDesc.Count; i++)
                regexSearchList[i] = new Regex(filterListDesc[i], regexOptions);


        }

        public static bool IsWord(string word) => filterList.Contains(word);
        public static List<string> GetWords() => filterList;

        public static void AddWord(string word)
        {
            filterList.Add(word);
            UpdatePatternString();
        }
        public static void RemoveWord(string word)
        {
            filterList.Remove(word);
            UpdatePatternString();
        }

        public static List<string> GetBadWords(string message, out string notatedMessage)
        {
            List<string> returnVal = new List<string>(); // Our sentinel value for no bad word is an empty List<string>.
            notatedMessage = String.Empty;
            StringBuilder sb = new StringBuilder(message);

            if (filterList.Count > 0)
            {
                foreach (Regex regexPattern in regexSearchList)
                {
                    MatchCollection mc = regexPattern.Matches(message);
                        //Matches(message, regexPattern, regexOptions);

                    if (mc.Count > 0)
                    {
                        int annoteSymbolsAdded = 0;
                        // Let's check every bad word
                        for (int i = 0; i < mc.Count; i++)
                        {
                            Match match = mc[i];
                            string badWord = match.Value;
                            int badWordIndex = match.Index;

                            if (!Excludes.IsExcluded(message, badWord, badWordIndex))
                            {
                                returnVal.Add(AttemptGetFullBadWord(message, badWordIndex, mc[i].Length, out int startIndex, out int endIndex));
                                sb.Insert(startIndex + annoteSymbolsAdded++, '%');
                                sb.Insert(endIndex + annoteSymbolsAdded++, '%');
                            }
                        }
                    }
                }
            }

            if (sb.Length > 0)
                notatedMessage = sb.ToString();

            if (notatedMessage.Length > 1000)
                notatedMessage = notatedMessage.Substring(0, 1000) + @"...";

            return returnVal;
        }

        private static string AttemptGetFullBadWord(string message, int badWordIndex, int badWordLength, out int startIndex, out int endIndex)
        {
            int i_;
            string returnVal;
            bool found_;

            // Get the index of the space before the word.
            startIndex = badWordIndex; // Default value if index not found.
            found_ = false; // We haven't found anything yet.
            i_ = badWordIndex; while (!found_ && --i_ >= 0)
            {
                if(message[i_] == ' ')
                {
                    startIndex = i_ + 1;
                    found_ = true;
                }
            }

            // Get the index of the space after the word.
            endIndex = message.Length; // Default value if index not found.
            found_ = false; // We haven't found anything yet.
            i_ = badWordIndex + badWordLength - 1; while(!found_ && ++i_ < message.Length)
            {
                if(i_ <= message.Length && message[i_] == ' ')
                {
                    endIndex = i_;
                    found_ = true;
                }
            }

            if (badWordIndex >= 0 && badWordIndex + badWordLength <= message.Length)
                returnVal = message.Substring(startIndex, endIndex - startIndex);
            else returnVal = message.Substring(badWordIndex, badWordLength);

            return returnVal;
        }

        static void OnFilterTriggered(FilterEventArgs e)
        {
            FilterTriggeredEventHandler handler = FilterTriggered;
            handler?.Invoke(e);
        }

        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            // Skip if (1) this channel is excluded or (2) this is sent by the bot.
            if (!BotSettings.IsChannelExcluded(e.Channel) && !e.Author.IsBot)
                CheckMessage(e.Message);
        }

        internal static async Task BotClient_MessageUpdated(MessageUpdateEventArgs e)
        {
            // Skip if (1) this channel is excluded or (2) this is sent by the bot.
            if (!BotSettings.IsChannelExcluded(e.Channel) && !e.Author.IsBot)
                CheckMessage(e.Message);
        }
        
        /// <summary>Check the messages for any Bad Words aka slurs.</summary>
        /// <param name="message">The message object to inspect.</param>
        private static void CheckMessage(DiscordMessage message)
        {
            // Let's check if the audit channel is set.
            if (BotSettings.FilterChannelId != 0)
            {
                string notatedMessage;
                List<string> badWords = GetBadWords(message.Content, out notatedMessage); // The detected bad words.

                if (badWords.Count > 0)
                {
                    OnFilterTriggered(
                        new FilterEventArgs
                        {
                            Message = message,
                            Channel = message.Channel,
                            User = message.Author,
                            BadWords = badWords.ToArray(),
                            NotatedMessage = notatedMessage
                        }) ;
                }
            }
        }
    }
}
