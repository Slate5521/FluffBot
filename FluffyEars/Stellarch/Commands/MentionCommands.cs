// MentionCommands.cs
// Contains commands for looking up mentions:
//  !mentions <users>
//  !mentions <userIds>
//
// EMIKO

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace BigSister.Commands
{
    class MentionCommands : BaseCommandModule
    {
        [Command("mentions"),
         MinimumRole(Role.CS),
         Description("WIP")]
        public async Task MentionsLookup(CommandContext ctx, params ulong[] users)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await MentionsLookupUnwrapper(ctx, users);
            }
        }

        [Command("mentions"),
                 MinimumRole(Role.CS),
                 Description("WIP")]
        public async Task MentionsLookup(CommandContext ctx, params DiscordUser[] users)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await MentionsLookupUnwrapper(ctx, users.Select(a => a.Id).ToArray());
            }
        }

        public async Task MentionsLookupUnwrapper(CommandContext ctx, ulong[] users)
        {
            await MentionSnooper.MentionSnooper.SeekWarns(ctx, users);
        }
    }
}
