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

        public static async Task LogSomething(DiscordUser who, string descriptor, string newValue)
        {
            DiscordChannel auditChan = await Bot.BotClient.GetChannelAsync(AUDIT_CHAN);

            StringBuilder sb = new StringBuilder();

            sb.Append(@"**");
            sb.Append(who.Username);
            sb.Append('#');
            sb.Append(who.Discriminator);
            sb.Append(@"** (");
            sb.Append(who.Mention);
            sb.Append(@") modified **");
            sb.Append(descriptor);
            sb.AppendLine(@"** with the value(s): ");
            sb.AppendLine(newValue);
            sb.Append(@"------------------------------------------------------");

            await auditChan.SendMessageAsync(sb.ToString());
        }
    }
}
