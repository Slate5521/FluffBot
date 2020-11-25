using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BigSister
{
    public class BotCommands : BaseCommandModule
    {
        [Command("dropped")]
        public async Task DroppedPotato(CommandContext ctx, [RemainingText] string twitchName)
        {
            const ulong CHANNEL_SPECIFIC = 780840642678358076;

            // Check if this is the streamer channel.
            if (ctx.Channel.Id == CHANNEL_SPECIFIC)
            {
                await Bot.PostAnnouncement($"{@"{0}"}\n:fire::potato:The potato has been dropped!:potato::fire: Come find out what happened and watch **{twitchName}** pick up from the ashes at {@"https://twitch.tv/"}{twitchName}");
            }
        }
    }
}
