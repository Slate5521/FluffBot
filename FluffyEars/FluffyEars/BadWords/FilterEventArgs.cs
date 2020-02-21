using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyEars.BadWords
{
    public class FilterEventArgs : EventArgs
    {
        public DiscordMessage Message;
        public DiscordUser User;
        public DiscordChannel Channel;
        public string[] BadWords;
    }
}
