using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluffyEars.Spam
{
    public static class SpamFilter
    {
        static Regex emoticonRegex = new Regex(@":\w:", 
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        const string emoticonReplaceChar = @" ";

        private static Queue<DiscordMessageInfo> last12Messages
            = new Queue<DiscordMessageInfo>(12);

        private static List<TimeoutInformation> SpamTimeout = new List<TimeoutInformation>();

        public static SpamDetectedEventHandler SpamDetected;
        public delegate void SpamDetectedEventHandler(SpamEventArgs e);
        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            bool spamFound = false;

            if (last12Messages.Count == 12)
                last12Messages.Dequeue();

            long messageTime = e.Message.CreationTimestamp.ToUnixTimeMilliseconds();
            ulong authorId = e.Message.Author.Id;

            last12Messages.Enqueue(
                new DiscordMessageInfo
                {
                    ChannelID = e.Message.ChannelId,
                    MessageID = e.Message.Id,
                    MessageTime = messageTime,
                    UserID = authorId,
                    MillisecondsSinceLastUserMessage = GetLastMessageMillisecondsElapsed(authorId, messageTime)
                });

            if (IsUserTimedOut(authorId) && GetRemainingTimeoutMilliseconds(authorId) <= 0)
                RemoveUserTimeout(authorId);

            if (!BotSettings.IsChannelExcluded(e.Channel))
            {
                // Check if bots etc.
                if (authorId != 451890906975698954 && // Ignore Wolfy
                   authorId != 346233847236788224 &&  // Ignore Muffy
                   !e.Author.IsBot &&                 // Ignore self 
                   !IsUserTimedOut(authorId))         // Ignore anyone who's being timed out.
                {
                    // ----------------------------------------------------------------
                    // Let's see if they're overflowing.

                    int maxMsgPerSec = BotSettings.MaxMessagesPerSecond;

                    // This means that, out of the 12 or so messages sent to the server recently, the threshold amount of them have been from this 
                    // user.
                    if (CountMessagesInSpan(authorId) > maxMsgPerSec)
                    {
                        // So we know this user is probably spamming. We need to check that for ourselves.

                        DiscordMessageInfo[] allUserSpanMessages = GetMessagesInSpan(authorId).Reverse().ToArray();

                        // We will loop in such a way that:
                        //  INITIALIZATION: We will...
                        //      ... FIRSTLY start at the first message index; that is _first_
                        //      ... SECONDLY get the last message index, offset the maxMsgPerSec; that is _last_
                        //  TEST EXPRESSION: We want to make sure that the last index never exceeds the array limits. If we cannot find a concurrent
                        //      maxMsgPerSec-length span of messages, that means we don't have a span of messages that may exceed the 
                        //      max-per-second.
                        //  INCREMENT: We will simply increment the first index and last index
                        for (int first = 0, last = first + maxMsgPerSec; //!
                            !spamFound && last < allUserSpanMessages.Length; //!
                            first++, last++)
                        {
                            // I don't necessarily care to distinguish between first or last because all I want is the difference between their sent
                            // times, and if that difference does not exceed 1000 ms.
                            DiscordMessageInfo a_ = allUserSpanMessages[first];
                            DiscordMessageInfo b_ = allUserSpanMessages[last];

                            // Found a spamThreshold span of messages where the difference in the last and first is <= 1000.
                            if (Math.Abs(a_.MessageTime - b_.MessageTime) <= 1000)
                            {
                                OnSpamDetected(new SpamEventArgs
                                {
                                    Channel = e.Channel,
                                    Message = e.Message,
                                    Spammer = e.Author
                                });

                                TimeoutUser(authorId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + BotSettings.SpamTimeout);

                                spamFound = true;
                            }
                        }
                    }

                    // ----------------------------------------------------------------
                    // Let's check if the message is over the limit.
                    if (!spamFound && e.Message.Content.Length > BotSettings.MaxMessageLength)
                    {
                        // If it goes over, let's give them the benefit of the doubt. Maybe they're Namsam, so let's make it so emoticons only take 
                        // up one character.
                        string newMessage = emoticonRegex.Replace(e.Message.Content, emoticonReplaceChar);

                        // If it's still over, let's notify.
                        if (newMessage.Length > BotSettings.MaxMessageLength)
                        {
                            OnSpamDetected(new SpamEventArgs
                            {
                                Channel = e.Channel,
                                Message = e.Message,
                                Spammer = e.Author
                            });
                            spamFound = true;
                        }
                    }

                    // ----------------------------------------------------------------
                    // Let's count how many linesplits there are.
                    if (!spamFound && e.Message.Content.Count(a => a == '\n') > BotSettings.MaxMessageSplits)
                    {
                        OnSpamDetected(new SpamEventArgs
                        {
                            Channel = e.Channel,
                            Message = e.Message,
                            Spammer = e.Author
                        });

                        spamFound = true;
                    }
                }
            }
        }

        private static void TimeoutUser(ulong id, long duration)
        {
            if (!IsUserTimedOut(id))
                SpamTimeout.Add(
                    new TimeoutInformation
                    {
                        TimeoutEndMilliseconds = duration,
                        UserId = id
                    });
        }

        private static bool IsUserTimedOut(ulong id)
        {
            return SpamTimeout.Any(a => a.UserId == id);
        }

        private static void RemoveUserTimeout(ulong id)
        {
            if(IsUserTimedOut(id))
            {
                TimeoutInformation timeoutStruct = 
                    SpamTimeout.Find(a => a.UserId == id);

                SpamTimeout.Remove(timeoutStruct);
            }
        }

        private static long GetRemainingTimeoutMilliseconds(ulong id)
        {
            return SpamTimeout.First(a => a.UserId == id).TimeoutEndMilliseconds
                - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static long GetLastMessageMillisecondsElapsed(ulong userId, long ms)
        {
            long returnVal = -1; // -1 is our sentinel value for "not found."

            foreach(DiscordMessageInfo info in last12Messages.Reverse())
            {
                if(returnVal == -1)
                {
                    // Same user id!
                    if (info.UserID == userId)
                        returnVal = ms - info.MessageTime;
                }
            }

            return returnVal;
        }

        /// <summary>Count how many messages have been sent by a user in the span of 12 messages.</summary>
        static int CountMessagesInSpan(ulong userId) =>
            last12Messages.Count(a => a.UserID == userId);

        static DiscordMessageInfo[] GetMessagesInSpan(ulong userId)
        {
            return
                (
                   from discordMsgI in last12Messages
                   where (discordMsgI.UserID == userId)
                   select discordMsgI
                   
                   ).ToArray();
        }

        static void OnSpamDetected(SpamEventArgs e)
        {
            SpamDetectedEventHandler handler = SpamDetected;
            handler?.Invoke(e);
        }
    }
}

