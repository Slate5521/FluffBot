using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyEars.Spam
{
    public struct DiscordMessageInfo
    {
        /// <summary>The ID of the user sending this message.</summary>
        public ulong UserID;
        /// <summary>The ID of the channel the message was sent to.</summary>
        public ulong ChannelID;
        /// <summary>The ID of the message.</summary>
        public ulong MessageID;
        /// <summary>The time in UTC Milliseconds the message was sent.</summary>
        public long MessageTime;
        /// <summary>
        /// How long it has been in Milliseconds since the user has sent his or her last message.
        /// </summary>
        public long MillisecondsSinceLastUserMessage;
    }
}
