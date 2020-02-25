// BadwordSystem.cs
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
        private static string regexPattern = String.Empty; // All of filterList in a pattern.

        public static void Default()
        {
            filterList = new List<string>();
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
            StringBuilder sb = new StringBuilder();

            //List<string> filterListDesc = filterList.OrderByDescending(a => a.Length).ToList();
            List<string> filterListDesc = filterList;
            filterListDesc.Sort();
            filterListDesc.Reverse();

            foreach (string pattern in filterListDesc)
            {
                sb.Append(pattern);

                // If this is not the last filter word, add an | separator. 
                if (!filterListDesc.Last().Equals(pattern))
                    sb.Append('|');
            }

            regexPattern = sb.ToString();
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

        public static List<string> GetBadWords(string message)
        {
            List<string> returnVal = new List<string>(); // Our sentinel value for no bad word is an empty List<string>.
                
            if (filterList.Count > 0)
            {
                MatchCollection mc = Regex.Matches(message, regexPattern, regexOptions);

                if (mc.Count > 0)
                {
                    // Let's check every bad word
                    for(int i = 0; i < mc.Count; i++)
                    {
                        Match match = mc[i];
                        string badWord = match.Value;
                        int badWordIndex = match.Index;

                        if (!Excludes.IsExcluded(message, badWord, badWordIndex))
                        {
                            returnVal.Add(AttemptGetFullBadWord(message, badWordIndex, mc[i].Length));
                        }
                    }
                }
            }

            return returnVal;
        }

        private static string AttemptGetFullBadWord(string message, int badWordIndex, int badWordLength)
        {
            int startIndex;
            int endIndex;
            int i_;
            string returnVal;
            bool found_;

            // Get the index of the space before the word.
            startIndex = badWordIndex; // Default value if index not found.
            found_ = false; // We haven't found anything yet.
            i_ = badWordIndex; while (--i_ >= 0 && !found_)
            {
                if(message[i_] == ' ')
                {
                    startIndex = i_;
                    found_ = true;
                }
            }

            // Get the index of the space after the word.
            endIndex = badWordIndex + badWordLength; // Default value if index not found.
            found_ = false; // We haven't found anything yet.
            i_ = badWordIndex + badWordLength; while(++i_ <= message.Length && !found_)
            {
                if(message[i_] == ' ')
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
                List<string> badWords = GetBadWords(message.Content); // The detected bad words.

                if (badWords.Count > 0)
                {
                    OnFilterTriggered(
                        new FilterEventArgs
                        {
                            Message = message,
                            Channel = message.Channel,
                            User = message.Author,
                            BadWords = badWords.ToArray()
                        });
                }
            }
        }
    }
}
