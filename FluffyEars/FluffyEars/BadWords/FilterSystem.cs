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

        /// <summary>Regex options for all instantiated Regexes in this class.</summary>
        private static RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
        /// <summary>A list of masks to use for Regexes.</summary>
        private static List<string> maskList;
        /// <summary>A list of Regex instances that uses masks from maskList.</summary>
        private static Regex[] regexList;

        /// <summary>The file to save Exclude information to.</summary>
        private const string BaseFile = "filter";
        /// <summary>The SaveFile object for this class.</summary>
        /// <see cref="SaveFile"/>
        private static SaveFile saveFile = new SaveFile(BaseFile);
        /// <summary>The lock object for this class' I/O operations.</summary>
        private static readonly object lockObj = (object)@"
         \\
          \\_
           (_)
          / )
   jgs  o( )_\_";

        #region Save/Load Methods

        /// <summary>Instantiate default values for this class.</summary>
        public static void Default()
        {
            maskList  = new List<string>();
            regexList = new Regex[0];

            Save();
        }

        /// <summary>Save the class to its save file.</summary>
        public static void Save()
        {
            saveFile.Save(maskList, lockObj);

            UpdateRegexList();
        }

        /// <summary>Checks if the expected save file for this class can be loaded from.</summary>
        public static bool CanLoad() 
            => saveFile.IsExistingSaveFile();

        /// <summary>Loads the save file or instantiates default values if unable to load.</summary>
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
        
        // Called whenever a message is created.
        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            // Skip if (1) this channel is excluded or (2) this is sent by the bot.
            if (!BotSettings.IsChannelExcluded(e.Channel) && !e.Author.IsBot)
                CheckMessage(e.Message);
        }

        // Called whenever a message is updated.
        internal static async Task BotClient_MessageUpdated(MessageUpdateEventArgs e)
        {
            // Skip if (1) this channel is excluded or (2) this is sent by the bot.
            if (!BotSettings.IsChannelExcluded(e.Channel) && !e.Author.IsBot)
                CheckMessage(e.Message);
        }
        
        #endregion Event Listeners
        // ################################
        #region Public Methods

        /// <summary>Checks if the specified string is a mask.</summary>
        public static bool IsMask(string mask) 
            => maskList.Contains(mask) || maskList.Contains(mask.Replace(@" ", @"\s"));
        
        /// <summary>Returns the mask list.</summary>
        public static List<string> GetMasks() 
            => maskList;

        /// <summary>Adds a mask to the filter system.</summary>
        public static void AddMask(string mask)
        {
            maskList.Add(mask);

            UpdateRegexList();
        }

        /// <summary>Removes a mask from the filter system.</summary>
        public static void RemoveMask(string mask)
        {
            maskList.Remove(mask);

            UpdateRegexList();
        }

        /// <summary>Invoked when the filter is triggered upon finding a bad word.</summary>
        public static async Task HandleFilterTriggered(FilterEventArgs e)
        {
            var stringBuilder = new StringBuilder();

            // Append all the found bad words to the string builder.
            foreach (string str in e.BadWords)
            {
                stringBuilder.Append(str);
                stringBuilder.Append(' ');
            }

            // Create the Discord Embed
            var deb = new DiscordEmbedBuilder();

            deb.WithTitle("Filter: Word Detected");
            deb.WithColor(DiscordColor.Red);

            deb.WithDescription(String.Format("Filter Trigger(s):```{0}```Excerpt:```{1}```",
                stringBuilder.ToString(), e.NotatedMessage));

            deb.AddField(@"Author ID", e.User.Id.ToString(), inline: true);
            deb.AddField(@"Author Username", $"{e.User.Username}#{e.User.Discriminator}", inline: true);
            deb.AddField(@"Author Mention", e.User.Mention, inline: true);
            deb.AddField(@"Channel", e.Channel.Mention, inline: true);
            deb.AddField(@"Timestamp (UTC)", e.Message.CreationTimestamp.UtcDateTime.ToString(), inline: true);
            deb.AddField(@"Link", ChatObjects.GetMessageUrl(e.Message));

            deb.WithThumbnail(ChatObjects.URL_FILTER_BUBBLE);

            // Notify the filter channel.
            await Bot.NotifyFilterChannel(deb.Build());
        }

        /// <summary>Get all the bad words in a message.</summary>
        /// <param name="message">The message to search.</param>
        /// <param name="notatedMessage">A string to notate, emphasizing where the bad words are.</param>
        public static List<string> GetBadWords(string message, out string notatedMessage)
        {
            var returnVal = new List<string>(); // Our sentinel value for no bad word is an empty List<string>.
            var stringBuilder = new StringBuilder(message); // Notated message string builder

            if (maskList.Count > 0)
            {
                int annoteSymbolsAdded = 0; // The number of annotation symbols added.

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

            // If the notated message is over 1500 characters, let's cut it down a little bit. I don't want to wait until 2,000 characters
            // specifically because imo that's risking it.
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
            // The maskList but in reverse alphabetical order.
            List<string> maskListDesc = maskList;

            maskListDesc.Sort();
            maskListDesc.Reverse();

            // Initiate a regex value for every mask.
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
        
        // When the filter is triggered.
        static void OnFilterTriggered(FilterEventArgs e)
        {
            FilterTriggeredEventHandler handler = FilterTriggered;
            handler?.Invoke(e);
        }
    }
}