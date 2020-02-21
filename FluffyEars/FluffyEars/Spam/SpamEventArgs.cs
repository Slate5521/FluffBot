using System;
using System.Collections.Generic;
using System.Text;
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
