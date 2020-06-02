using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluffyEars.Commands
{
    class RequestedCommands : BaseCommandModule
    {
        [Command("userinfo")]
        public async Task GetUserInfo(CommandContext ctx, DiscordMember member)
        {
            if (ctx.Member.GetHighestRole().IsCHOrHigher())
            {

                await ctx.Channel.TriggerTypingAsync();

                if (ctx.Guild.Members.ContainsKey(member.Id))
                {
                    // DEB!
                    var deb = new DiscordEmbedBuilder();

                    deb.WithColor(DiscordColor.Aquamarine);
                    deb.WithTitle("User Info");
                    deb.WithThumbnail(member.AvatarUrl);

                    deb.AddField(@"Joined Discord:", 
                        GetDateString(GetJoinedDiscordTime(member.Id)));
                    deb.AddField($"Joined {ctx.Guild.Name}:", GetDateString(member.JoinedAt));

                    await ctx.Channel.SendMessageAsync(embed: deb);
                }
                else
                    await ctx.Channel.SendMessageAsync(ChatObjects.GetErrMessage("User not found!"));
            }

        }

        private static string GetDateString(DateTimeOffset dto)
        {
            const string commaSpace = @", ";

            TimeSpan timeSpan = DateTimeOffset.Now.Subtract(dto);
            StringBuilder sb = new StringBuilder();
            bool placeCommaSpace = false;

            sb.AppendLine(dto.ToString());
            sb.Append(@" (");
            
            if(timeSpan.Days > 0)
            {
                sb.Append(timeSpan.Days);
                sb.Append(@" day");

                if (timeSpan.Days > 1)
                    sb.Append('s');

                placeCommaSpace = true;
            }

            if (timeSpan.Hours > 0)
            {
                if (placeCommaSpace)
                {
                    sb.Append(commaSpace);
                    placeCommaSpace = false;
                }

                sb.Append(timeSpan.Hours);
                sb.Append(@" hour");

                if (timeSpan.Hours > 1)
                    sb.Append('s');

                placeCommaSpace = true;
            }

            if (timeSpan.Minutes > 0)
            {
                if (placeCommaSpace)
                {
                    sb.Append(commaSpace);
                    sb.Append(@"and ");
                }

                sb.Append(timeSpan.Minutes);
                sb.Append(@" minutes");

                if (timeSpan.Minutes > 1)
                    sb.Append(' ');
            }

            sb.Append(@" ago)");

            return sb.ToString();
        }

        private static DateTimeOffset GetJoinedDiscordTime(ulong id)
        {
            return DateTimeOffset
                .FromUnixTimeMilliseconds((long)(id >> 22) + 1420070400000);
        }
    }
}
