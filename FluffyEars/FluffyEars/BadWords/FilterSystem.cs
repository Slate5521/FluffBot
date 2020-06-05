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

        public const string BaseFile = "filter";
        private static readonly object lockObj = (object)@"
         \\
          \\_
           (_)
          / )
   jgs  o( )_\_";
        private static SaveFile saveFile = new SaveFile(BaseFile);
        static RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;

        private static List<string> maskList;
        private static Regex[] regexList;

        public static void Default()
        {
            maskList = new List<string>();
            regexList = new Regex[0];

            Save();
        }
        public static void Save()
        {
            saveFile.Save<List<string>>(maskList, lockObj);

            UpdateRegexList();
        }
        public static bool CanLoad() => saveFile.IsExistingSaveFile();
        public static void Load()
        {
            if (CanLoad())
            {
                maskList = saveFile.Load<List<string>>(lockObj);
                UpdateRegexList();
            }
            else Default();
        }

        /// <summary>This updates the RegEx megastring, containing all the filter words but in a word1|word2 format.</summary>
        private static void UpdateRegexList()
        {
            List<string> maskListDesc = maskList;
            maskListDesc.Sort();
            maskListDesc.Reverse();
            
            regexList = new Regex[maskListDesc.Count];

            for (int i = 0; i < maskListDesc.Count; i++)
                regexList[i] = new Regex(maskListDesc[i], regexOptions);
        }

        public static bool IsMask(string mask) => maskList.Contains(mask) || maskList.Contains(mask.Replace(@" ", @"\s"));
        public static List<string> GetMasks() => maskList;

        public static void AddMask(string mask)
        {
            maskList.Add(mask);
            UpdateRegexList();
        }
        public static void RemoveMask(string mask)
        {
            maskList.Remove(mask);
            UpdateRegexList();
        }

        public static List<string> GetBadWords(string message, out string notatedMessage)
        {
            List<string> returnVal = new List<string>(); // Our sentinel value for no bad word is an empty List<string>.
            StringBuilder sb = new StringBuilder(message);

            if (maskList.Count > 0)
            {
                int annoteSymbolsAdded = 0;

                foreach (Regex regexPattern in regexList)
                {
                    MatchCollection mc = regexPattern.Matches(message);

                    if (mc.Count > 0)
                    {
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

            notatedMessage = sb.ToString();

            if (sb.Length > 1500)
                notatedMessage = $"{notatedMessage.Substring(0, 1500)}...\n**message too long to preview.**";

            return returnVal;
        }

        private static string AttemptGetFullBadWord(string message, int badWordIndex, int badWordLength, out int startIndex, out int endIndex)
        {
            int i;
            string returnVal;
            bool found;

            // Get the index of the space before the word.
            startIndex = badWordIndex; // Default value if index not found.
            found = false; // We haven't found anything yet.
            i = badWordIndex; 

            while (!found && --i >= 0)
            {
                if(message[i] == ' ')
                {
                    startIndex = i + 1;
                    found = true;
                }
            }

            // Get the index of the space after the word.
            endIndex = message.Length; // Default value if index not found.
            found = false; // We haven't found anything yet.
            i = badWordIndex + badWordLength - 1; 
            
            while(!found && ++i < message.Length)
            {
                if(i <= message.Length && message[i] == ' ')
                {
                    endIndex = i;
                    found = true;
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
