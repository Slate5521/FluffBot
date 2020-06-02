// SelfAudit.cs
// The bot will audit changes made to it.

using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluffyEars
{
    public static class SelfAudit
    {
        const ulong AUDIT_CHAN = 680252154631684110;

        public static async Task LogSomething(DiscordUser who, string messageUrl, string description, string command, string arguments, DiscordColor color)
        {
            // DEB!
            DiscordEmbedBuilder deb = new DiscordEmbedBuilder();
            deb.WithColor(color);

            deb.AddField(@"User", who.Mention, true);
            deb.AddField(@"User ID", $"{who.Username}#{who.Discriminator}", true);
            deb.AddField(@"Snowflake", who.Id.ToString(), true);
            deb.AddField(@"Command", command, true);

            if (arguments.Length > 0)
                deb.AddField(@"Arguments", command, true);

            deb.AddField(@"Link", messageUrl, true);

            DiscordChannel auditChan = await Bot.BotClient.GetChannelAsync(AUDIT_CHAN);

            await auditChan.SendMessageAsync(embed: deb.Build());
        }
    }
}
