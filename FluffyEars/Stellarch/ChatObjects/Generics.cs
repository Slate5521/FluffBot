using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BigSister.ChatObjects
{
    public static class Generics
    {
        const string EMPTY_STRING = @"";

        const string NEGATIVE_PREFIX = @":x: **/(u︵u)\\**";
        const string POSITIVE_PREFIX = @":white_check_mark: **/(^‿^)\\**";
        const string NEUTRAL_PREFIX = @":speech_left: **/('▿')\\**";

        public static DiscordColor PositiveColor = DiscordColor.Green;
        public static DiscordColor NegativeColor = DiscordColor.Red;
        public static DiscordColor NeutralColor = DiscordColor.MidnightBlue;

        public const string URL_SPEECH_BUBBLE = @"https://i.imgur.com/NAHI4h3.png";
        public const string URL_FILTER_ADD = @"https://i.imgur.com/cVeWPNz.png";
        public const string URL_FILTER_SUB = @"https://i.imgur.com/kNdFBoK.png";
        public const string URL_FILTER_BUBBLE = @"https://i.imgur.com/TAaRDgI.png";
        public const string URL_FILTER_GENERIC = @"https://i.imgur.com/58C8Noh.png";
        public const string URL_REMINDER_GENERIC = @"https://i.imgur.com/Ya1Lu6e.png";
        public const string URL_REMINDER_EXCLAIM = @"https://i.imgur.com/NUCrbSl.png";
        public const string URL_REMINDER_DELETED = @"https://i.imgur.com/OnTaJdd.png";

        /// <summary>A generic embed template.</summary>
        public static DiscordEmbedBuilder GenericEmbedTemplate(DiscordColor? color,
                                                               string description = EMPTY_STRING,
                                                               string thumbnail = EMPTY_STRING,
                                                               string title = EMPTY_STRING)
        {
            var embedBuilder = new DiscordEmbedBuilder();

            embedBuilder.WithColor(color.HasValue ? color.Value : NeutralColor);

            if (description.Length > 0)
                embedBuilder.WithDescription(description);
            if (title.Length > 0)
                embedBuilder.WithTitle(title);

            embedBuilder.WithThumbnail(thumbnail.Length > 0 ? thumbnail : URL_SPEECH_BUBBLE);

            return embedBuilder;
        }

        /// <summary>A template for Floppy directly responding to a user.</summary>
        public static string NegativeDirectResponseTemplate(string mention, string body)
            => $"{NEGATIVE_PREFIX} {mention}, {body}";

        public static string ExceptionDirectResponseTemplate()
            => throw new NotImplementedException();
    }
}
