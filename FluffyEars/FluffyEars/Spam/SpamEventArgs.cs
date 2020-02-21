// SpamEventArgs.cs
// Just some args for when the spam filter is triggered.

using System;
using DSharpPlus.Entities;

namespace FluffyEars.Spam
{
    public class SpamEventArgs : EventArgs
    {
        public DiscordMessage Message;
        public DiscordUser Spammer;
        public DiscordChannel Channel;
    }
}
