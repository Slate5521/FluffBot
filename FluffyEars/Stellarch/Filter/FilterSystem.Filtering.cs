﻿// FilterSystem.Filtering.cs
// A portion of the filter system containing everything needed for processing commands from FilterCommands.cs
// 1) Receives all message creation/edit events and filters them.
// 2) Raises events whenever the filter is triggered.
//
// EMIKO

using BigSister.Settings;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BigSister.Filter
{
    public static partial class FilterSystem
    {
        /// <summary>Invoked when the filter is triggered.</summary>
        public static event FilterTriggeredEventHandler FilterTriggered;
        public delegate void FilterTriggeredEventHandler(FilterEventArgs e);

        // Disable compiler warning for async code not having await. DSharpPlus demands that these two methods be an awaitable type, but I want them
        // to be synchronous. Thanks a lot.
        #pragma warning disable CS1998
        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            // Only continue if the channel isn't excluded, if the sender isn't the bot, and if this isn't sent in PMs.
            if(!Program.Settings.ExcludedChannels.Contains(e.Channel.Id) && 
               !e.Channel.IsPrivate &&
               !e.Author.IsCurrent)
            {
                CheckMessage(e.Message);
            }
        }
        internal static async Task BotClient_MessageUpdated(MessageUpdateEventArgs e)
        {
            // Only continue if the channel isn't excluded, if the sender isn't the bot, and if this isn't sent in PMs.
            if (!Program.Settings.ExcludedChannels.Contains(e.Channel.Id) &&
               !e.Channel.IsPrivate &&
               !e.Author.IsCurrent)
            {
                CheckMessage(e.Message);
            }
        }
        #pragma warning restore CS1998

        /// <summary>Check the message against the filter.</summary>
        private static void CheckMessage(DiscordMessage message)
        {
            // Let's check if the audit channel is set.
            if (Program.Settings.FilterChannelId != 0)
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
                        });
                }
            }
        }

        /// <summary>Get all the bad words in a message.</summary>
        /// <param name="message">The message to search.</param>
        /// <param name="notatedMessage">A string to notate, emphasizing where the bad words are.</param>
        public static List<string> GetBadWords(string message, out string notatedMessage)
        {
            var returnVal = new List<string>(); // Our sentinel value for no bad word is an empty List<string>.
            var stringBuilder = new StringBuilder(message); // Notated message string builder

            if (MaskCache.Length > 0)
            {
                int annoteSymbolsAdded = 0; // The number of annotation symbols added.

                foreach (Regex regexPattern in FilterRegex)
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

                            if (!IsExcluded(message, badWord, badWordIndex))
                            {
                                returnVal.Add(badWord);

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

        // When the filter is triggered.
        static void OnFilterTriggered(FilterEventArgs e)
        {
            FilterTriggeredEventHandler handler = FilterTriggered;
            handler?.Invoke(e);
        }
    }
}
