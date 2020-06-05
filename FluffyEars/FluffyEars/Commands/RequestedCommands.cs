using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FluffyEars.Commands
{
    class RequestedCommands : BaseCommandModule
    {
        [Command("userinfo")]
        public async Task GetUserInfo(CommandContext ctx, DiscordMember member)
        {
            if (!ctx.Member.GetHighestRole().IsCSOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                       (
                           requiredRole: Role.CS,
                           command: ctx.Command.Name,
                           channel: ctx.Channel,
                           caller: ctx.Member
                       );
            }
            else
            {
                await ctx.Channel.TriggerTypingAsync();

                if (ctx.Guild.Members.ContainsKey(member.Id))
                {
                    // DEB!
                    var deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: @"User Info",
                            description: ChatObjects.GetNeutralMessage(@"Here's some info!"),
                            color: ChatObjects.NeutralColor,
                            thumbnail: member.AvatarUrl
                        ));

                    deb.AddField(@"Joined Discord:", GetDateString(GetJoinedDiscordTime(member.Id)));
                    deb.AddField($"Joined {ctx.Guild.Name}:", GetDateString(member.JoinedAt));

                    await ctx.Channel.SendMessageAsync(embed: deb);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(
                        embed:
                        
                        ChatObjects.FormatEmbedResponse
                        (
                            title: @"Cannot Get User Info",
                            description: ChatObjects.GetErrMessage($"Unable to find that user..."),
                            color: ChatObjects.ErrColor,
                            thumbnail: member.AvatarUrl
                        ));
                }
            }

        }

        private static string GetDateString(DateTimeOffset dto)
        {
            const string commaSpace = @", ";

            var timeSpan = DateTimeOffset.Now.Subtract(dto);
            var stringBuilder = new StringBuilder();
            bool placeCommaSpace = false;

            stringBuilder.AppendLine(dto.ToString());
            stringBuilder.Append(@" (");
            
            if(timeSpan.Days > 0)
            {
                stringBuilder.Append(timeSpan.Days);
                stringBuilder.Append(@" day");

                if (timeSpan.Days > 1)
                {
                    stringBuilder.Append('s');
                }

                placeCommaSpace = true;
            }

            if (timeSpan.Hours > 0)
            {
                if (placeCommaSpace)
                {
                    stringBuilder.Append(commaSpace);
                    placeCommaSpace = false;
                }

                stringBuilder.Append(timeSpan.Hours);
                stringBuilder.Append(@" hour");

                if (timeSpan.Hours > 1)
                {
                    stringBuilder.Append('s');
                }

                placeCommaSpace = true;
            }

            if (timeSpan.Minutes > 0)
            {
                if (placeCommaSpace)
                {
                    stringBuilder.Append(commaSpace);
                    stringBuilder.Append(@"and ");
                }

                stringBuilder.Append(timeSpan.Minutes);
                stringBuilder.Append(@" minute");

                if (timeSpan.Minutes > 1)
                {
                    stringBuilder.Append('s');
                }
            }

            if (timeSpan.Seconds > 0)
            {
                if (placeCommaSpace)
                {
                    stringBuilder.Append(commaSpace);
                    stringBuilder.Append(@"and ");
                }

                stringBuilder.Append(timeSpan.Seconds);
                stringBuilder.Append(@" second");

                if (timeSpan.Seconds > 1)
                {
                    stringBuilder.Append('s');
                }
            }

            stringBuilder.Append(@" ago)");

            return stringBuilder.ToString();
        }

        private static DateTimeOffset GetJoinedDiscordTime(ulong id)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds((long)(id >> 22) + 1420070400000);
        }
    }
}
