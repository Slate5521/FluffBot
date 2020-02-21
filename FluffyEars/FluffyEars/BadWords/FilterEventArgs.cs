// FilterEventArgs.cs
// Contains arguments detailing the bad words found and other context in a filter event.

using DSharpPlus.Entities;
using System;

namespace FluffyEars.BadWords
{
    public class FilterEventArgs : EventArgs
    {
        public DiscordMessage Message;
        public DiscordUser User;
        public DiscordChannel Channel;
        /// <summary>The bad words found by the filter.</summary>
        public string[] BadWords;
    }
}
