// SpamFilter.cs
// Everything necessary to run the spam side of the filter system.

using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluffyEars.Spam
{
    public static class SpamFilter
    {
        /// <summary>This is what an emoticon looks like to RegEx.</summary>
        static Regex emoticonRegex = new Regex(@":\w:", 
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        /// <summary>Replace emoticons with this.</summary>
        const string emoticonReplaceChar = @" ";

        /// <summary>The last 12 messages ever sent by anyone in the server.</summary>
        /// I chose specifically 12 because it seems like a good number. If someone's spamming away, even at peak hours, they're definitely going to
        /// take up a sizeable chunk of this queue.
        private static Queue<DiscordMessageInfo> last12Messages
            = new Queue<DiscordMessageInfo>(12);

        /// <summary>A list of users being timed out, preventing them from retriggering the filter.</summary>
        private static List<TimeoutInformation> SpamTimeout = new List<TimeoutInformation>();

        /// <summary>Invoked when the spam filter is triggered.</summary>
        public static event SpamDetectedEventHandler SpamDetected;
        public delegate void SpamDetectedEventHandler(SpamEventArgs e);
        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            bool spamFound = false;

            // At this point, we want to keep the last 12 messages at 12 messages.
            if (last12Messages.Count == 12)
                last12Messages.Dequeue();

            long messageTime = e.Message.CreationTimestamp.ToUnixTimeMilliseconds();
            ulong authorId = e.Message.Author.Id;

            // Add a message's info to the queue.
            last12Messages.Enqueue(
                new DiscordMessageInfo
                {
                    ChannelID = e.Message.ChannelId,
                    MessageID = e.Message.Id,
                    MessageTime = messageTime,
                    UserID = authorId,
                    MillisecondsSinceLastUserMessage = GetLastMessageMillisecondsElapsed(authorId, messageTime)
                });

            // If a user is timed out, and they're not supposed to be timed out, let's remove them from the timeout collection before continuing.
            if (IsUserTimedOut(authorId) && GetRemainingTimeoutMilliseconds(authorId) <= 0)
                RemoveUserTimeout(authorId);

            // Check if this cannel is excluded.
            if (!BotSettings.IsChannelExcluded(e.Channel))
            {
                // Check if bots etc.
                if (authorId != 451890906975698954 && // Ignore Wolfy
                   authorId != 346233847236788224 &&  // Ignore Muffy
                   !e.Author.IsBot &&                 // Ignore self 
                   !IsUserTimedOut(authorId))         // Ignore anyone who's being timed out.
                {
                    // ----------------------------------------------------------------
                    // Let's see if they're overflowing. aka sending a fuckton of messages at once.

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

        /// <summary>Adds the user to the time out collection, preventing them from retriggering the bot for a while.</summary>
        /// <param name="duration">Duration in milliseconds.</param>
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

        /// <summary>Checks if the user is timed out currently.</summary>
        private static bool IsUserTimedOut(ulong id)
        {
            return SpamTimeout.Any(a => a.UserId == id);
        }

        /// <summary>Removes the user from the timeout collection, allowing them to trigger the spam system.</summary>
        private static void RemoveUserTimeout(ulong id)
        {
            if(IsUserTimedOut(id))
            {
                TimeoutInformation timeoutStruct = 
                    SpamTimeout.Find(a => a.UserId == id);

                SpamTimeout.Remove(timeoutStruct);
            }
        }

        /// <summary>Checks how much longer this person is being timed out for in milliseconds.</summary>
        private static long GetRemainingTimeoutMilliseconds(ulong id)
        {
            return SpamTimeout.First(a => a.UserId == id).TimeoutEndMilliseconds
                - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>Checks how long it has been between the user's current message and their last message.</summary>
        /// <param name="ms">The milliseconds of the most recent message.</param>
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

        /// <summary>Gets a collection of messages from the user in the last 12 message span.</summary>\
        static DiscordMessageInfo[] GetMessagesInSpan(ulong userId)
        {
            return
                (
                   from discordMsgI in last12Messages
                   where (discordMsgI.UserID == userId)
                   select discordMsgI
                   
                   ).ToArray();
        }

        /// <summary>SPAM!!!!!!!!!!!!!!!!!!!!!!</summary>
        static void OnSpamDetected(SpamEventArgs e)
        {
            SpamDetectedEventHandler handler = SpamDetected;
            handler?.Invoke(e);
        }
    }
}

