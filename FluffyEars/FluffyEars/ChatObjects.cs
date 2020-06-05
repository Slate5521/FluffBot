// ChatObjects.cs
// Various objects to be sent to chat.

using DSharpPlus.Entities;

namespace FluffyEars
{
    public static class ChatObjects
    {
        const string ERROR_PREFIX = @":x: **/(u︵u)\\**";
        const string SUCCESS_PREFIX = @":white_check_mark: **/(^‿^)\\**";
        const string NEUTRAL_PREFIX = @":speech_left: **/('▿')\\**";

        public const string URL_SPEECH_BUBBLE =    @"https://i.imgur.com/NAHI4h3.png";
        public const string URL_FILTER_ADD =       @"https://i.imgur.com/cVeWPNz.png";
        public const string URL_FILTER_SUB =       @"https://i.imgur.com/kNdFBoK.png";
        // public const string URL_FILTER_SPAM =      @"https://i.imgur.com/jh741I7.png";
        public const string URL_FILTER_BUBBLE =    @"https://i.imgur.com/TAaRDgI.png";
        public const string URL_FILTER_GENERIC =   @"https://i.imgur.com/58C8Noh.png";
        public const string URL_REMINDER_GENERIC = @"https://i.imgur.com/Ya1Lu6e.png";
        public const string URL_REMINDER_EXCLAIM = @"https://i.imgur.com/NUCrbSl.png";
        public const string URL_REMINDER_DELETED = @"https://i.imgur.com/OnTaJdd.png";

        public static DiscordColor SuccessColor = DiscordColor.Green;
        public static DiscordColor ErrColor = DiscordColor.Red;
        public static DiscordColor NeutralColor = DiscordColor.MidnightBlue;

        public static string GetSuccessMessage(string message)
            => $"{SUCCESS_PREFIX} {message}";
        public static string GetErrMessage(string message)
            => $"{ERROR_PREFIX} {message}";
        public static string GetNeutralMessage(string message)
            => $"{NEUTRAL_PREFIX} {message}";

        public static string GetMessageUrl(DiscordMessage message)
            => GetMessageUrl(message.Channel.GuildId, message.ChannelId, message.Id);
        public static string GetMessageUrl(ulong guildId, ulong channelId, ulong messageId)
            => $"https://discordapp.com/channels/{guildId}/{channelId}/{messageId}";

        public static string GetMention(ulong userId)
            => $"<@{userId}>";

        public static DiscordEmbed FormatEmbedResponse(string title, string description, DiscordColor color, string thumbnail = @"", params DiscordEmbedField[] fields)
        {
            var deb = new DiscordEmbedBuilder();

            deb.WithTitle(title);
            deb.WithDescription(description);
            deb.WithColor(color);

            if (thumbnail.Length > 0)
                deb.WithThumbnail(thumbnail);

            foreach(var field in fields)
            {
                deb.AddField
                    (
                        name:   field.Name,
                        value:  field.Value,
                        inline: field.Inline
                    );
            }

            return deb.Build();
        }
    }
}
