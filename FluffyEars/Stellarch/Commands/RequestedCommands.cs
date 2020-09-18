// RequestedCommands.cs
// Here are some commands requested by users.
//
// !userinfo which displays information about account creation date and server join date.
// 
// EMIKO

using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using BigSister.ChatObjects;

namespace BigSister.Commands
{
    [Group("Moderation-Requested")]
    class RequestedCommands : BaseCommandModule
    {
        /// <summary>Gets information about a user's account creation date and server join date.</summary>
        [Command("userinfo"),
         MinimumRole(Role.CS),
         Description("Gets a user's join date and creation date.\n\n**Usage:** !userinfo <mention/id>\n" +
            "*e.g. !userinfo <@131626628211146752> or !userinfo 131626628211146752*")]
        public async Task GetUserInfo(CommandContext ctx, DiscordMember member)
        {
            const string RECENT_MSG = @"In the last minute...";
            // Check if the user can use commands.
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                
                await ctx.Channel.TriggerTypingAsync();

                if (ctx.Guild.Members.ContainsKey(member.Id))
                {   // This member exists in the guild.
                    // DEB!
                    var deb = Generics.GenericEmbedTemplate(
                        color: Generics.NeutralColor,
                        description: Generics.NeutralDirectResponseTemplate(
                            mention: ctx.Member.Mention,
                            body: $"here's some info about that user!"),
                        title: @"User Info",
                        thumbnail: member.AvatarUrl);

                    deb.AddField(@"Mention", member.Mention, true);
                    deb.AddField(@"Username", $"{member.Username}#{member.Discriminator}", true);
                    deb.AddField(@"Joined Discord", Generics.GetRemainingTime(GetJoinedDiscordTime(member.Id), false, RECENT_MSG, @"ago"));
                    deb.AddField($"Joined {ctx.Guild.Name}", Generics.GetRemainingTime(member.JoinedAt, false, RECENT_MSG, @"ago"));

                    await ctx.Channel.SendMessageAsync(embed: deb);
                }
                else
                {   // This member does not exist in the guild.
                    await ctx.Channel.SendMessageAsync(
                        embed: Generics.GenericEmbedTemplate(
                            color: Generics.NegativeColor,
                            description: @"Unable to find that user...",
                            thumbnail: member.AvatarUrl,
                            title: @"Cannot get user info"));
                } // end else
            } // end if
        } // end method

        /// <summary>Retrieves the user's Discord join date by their snowflake.</summary>
        /// <param name="id">Snowflake</param>
        private static DateTimeOffset GetJoinedDiscordTime(ulong id)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds((long)(id >> 22) + 1420070400000);
        }
    }
}
