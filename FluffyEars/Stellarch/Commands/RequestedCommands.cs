// RequestedCommands.cs
// Here are some commands requested by users.
//
// !userinfo which displays information about account creation date and server join date.
// 
// EMIKO

using BigSister.ChatObjects;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BigSister.Commands
{
    class RequestedCommands : BaseCommandModule
    {
        /// <summary>Gets information about a user's account creation date and server join date.</summary>
        [Command("userinfo"),
         MinimumRole(Role.CS),
         Description("WIP")]
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
                            body: $"Here's some info about {member.Username}#{member.Discriminator}!"),
                        title: @"User Info",
                        thumbnail: member.AvatarUrl
                        );

                    deb.AddField(@"Joined Discord:", Generics.GetRemainingTime(GetJoinedDiscordTime(member.Id), false, RECENT_MSG, @"ago"));
                    deb.AddField($"Joined {ctx.Guild.Name}:", Generics.GetRemainingTime(member.JoinedAt, false, RECENT_MSG, @"ago"));

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
