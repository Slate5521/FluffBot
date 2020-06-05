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

        private static readonly object lockObj = (object)@"
         \\
          \\_
           (_)
          / )
   jgs  o( )_\_";
        private static SaveFile saveFile = new SaveFile(BaseFile);
        private static RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;

        private static List<string> maskList;
        private static Regex[] regexList;

        private const string BaseFile = "filter";

        #region Save/Load Methods

        public static void Default()
        {
            maskList  = new List<string>();
            regexList = new Regex[0];

            Save();
        }

        public static void Save()
        {
            saveFile.Save(maskList, lockObj);

            UpdateRegexList();
        }

        public static bool CanLoad() 
            => saveFile.IsExistingSaveFile();

        public static void Load()
        {
            if (CanLoad())
            {
                maskList = saveFile.Load<List<string>>(lockObj);

                UpdateRegexList();
            }
            else
            {
                Default();
            }
        }

        #endregion Save/Load Methods
        // ################################
        #region Event Listeners

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
        
        #endregion Event Listeners
        // ################################
        #region Public Methods

        public static bool IsMask(string mask) 
            => maskList.Contains(mask) || maskList.Contains(mask.Replace(@" ", @"\s"));
        
        public static List<string> GetMasks() 
            => maskList;

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

        public static async Task HandleFilterTriggered(FilterEventArgs e)
        {
            var stringBuilder = new StringBuilder();

            foreach (string str in e.BadWords)
            {
                stringBuilder.Append(str);
                stringBuilder.Append(' ');
            }

            // DEB!
            var deb = new DiscordEmbedBuilder();

            deb.WithTitle("Filter: Word Detected");
            deb.WithColor(DiscordColor.Red);

            deb.WithDescription(String.Format("Filter Trigger(s):```{0}```Excerpt:```{1}```",
                stringBuilder.ToString(), e.NotatedMessage));

            //deb.WithDescription(String.Format("{0} has triggered the filter system in {1}.", e.User.Mention, e.Channel.Mention));

            deb.AddField(@"Author ID", e.User.Id.ToString(), inline: true);
            deb.AddField(@"Author Username", e.User.Username + '#' + e.User.Discriminator, inline: true);
            deb.AddField(@"Author Mention", e.User.Mention, inline: true);
            deb.AddField(@"Channel", e.Channel.Mention, inline: true);
            deb.AddField(@"Timestamp (UTC)", e.Message.CreationTimestamp.UtcDateTime.ToString(), inline: true);
            deb.AddField(@"Link", ChatObjects.GetMessageUrl(e.Message));

            deb.WithThumbnail(ChatObjects.URL_FILTER_BUBBLE);

            await Bot.NotifyFilterChannel(deb.Build());
        }

        public static List<string> GetBadWords(string message, out string notatedMessage)
        {
            var returnVal = new List<string>(); // Our sentinel value for no bad word is an empty List<string>.
            var stringBuilder = new StringBuilder(message);

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
                            
                            returnVal.Add(badWord);

                            if (!Excludes.IsExcluded(message, badWord, badWordIndex))
                            {
                                stringBuilder.Insert(badWordIndex + annoteSymbolsAdded++, '%');
                                stringBuilder.Insert(badWordIndex + badWord.Length + annoteSymbolsAdded++, '%');
                            } // end if
                        } // end for
                    } // end if
                } // end foreach
            } // end if

            notatedMessage = stringBuilder.ToString();

            if (stringBuilder.Length > 1500)
            {
                notatedMessage = $"{notatedMessage.Substring(0, 1500)}...\n**message too long to preview.**";
            }

            return returnVal;
        }

        #endregion Public Methods
        // ################################
        #region Private Methods
        /// <summary>This updates the RegEx megastring, containing all the filter words but in a word1|word2 format.</summary>
        private static void UpdateRegexList()
        {
            var maskListDesc = maskList;

            maskListDesc.Sort();
            maskListDesc.Reverse();

            regexList = new Regex[maskListDesc.Count];

            for (int i = 0; i < maskListDesc.Count; i++)
            {
                regexList[i] = new Regex(maskListDesc[i], regexOptions);
            }
        }

        /// <summary>Check the messages for any Bad Words aka slurs.</summary>
        /// <param name="message">The message object to inspect.</param>
        private static void CheckMessage(DiscordMessage message)
        {
            // Let's check if the audit channel is set.
            if (BotSettings.FilterChannelId != 0)
            {
                string notatedMessage;

                var badWords = GetBadWords(message.Content, out notatedMessage); // The detected bad words.

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

        #endregion Private Methods
        // ################################
        // Event Handler
        static void OnFilterTriggered(FilterEventArgs e)
        {
            FilterTriggeredEventHandler handler = FilterTriggered;
            handler?.Invoke(e);
        }

        internal static Task HandleFilterTriggered()
        {
            throw new NotImplementedException();
        }
    }
}