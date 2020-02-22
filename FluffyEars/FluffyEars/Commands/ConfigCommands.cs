// ConfigCommands.cs
// Contains a module for modifying the general config.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace FluffyEars.Commands
{
    class ConfigCommands : BaseModule
    {
        [Command("setfilterchan")]
        public async Task SetFilterChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use these sorts of commands.
            if (ctx.Member.GetRole().IsBotManagerOrHigher())
            {
                await ctx.TriggerTypingAsync();

                // Cancels:
                if (BotSettings.FilterChannelId == chan.Id)
                {   // Trying to set the channel to itself.
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Unable to set filter channel. That's already the channel..."));
                    return;
                } 
                if (!ctx.Guild.Channels.Contains(chan))
                {   // Channel does not exist.
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Unable to set filter channel. Does not exist or is not in this guild..."));
                    return;
                }

                // Update the settings.
                BotSettings.FilterChannelId = chan.Id;
                BotSettings.Save();

                // Success message.
                await ctx.Channel.SendMessageAsync(
                    ChatObjects.GetSuccessMessage(
                        String.Format("Filter channel set to {0}!", chan.Mention)));
                await SelfAudit.LogSomething(ctx.User, @"filter channel", chan.Name);
            }
        }

        [Command("+chan"),
            Aliases("+channel")]
        public async Task IncludeChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use config commands.
            if (ctx.Member.GetRole().IsBotManagerOrHigher())
            {
                await ctx.Channel.TriggerTypingAsync();

                // Cancels:
                if(!BotSettings.IsChannelExcluded(chan))
                {   // This channel does not exist.
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Unable to un-exclude channel. This channel is not already excluded..."));
                    return;
                }
                if(!ctx.Guild.Channels.Contains(chan))
                {   // This channel is not in the guild.
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Unable to un-exclude channel. This channel does not exist in this server..."));
                    return;
                }

                BotSettings.IncludeChannel(chan);
                BotSettings.Save();

                await ctx.Channel.SendMessageAsync(
                    ChatObjects.GetSuccessMessage(
                        String.Format("{0} was successfully un-excluded!", chan.Mention)));
                await SelfAudit.LogSomething(ctx.User, @" + chan", chan.Name);
            }
        }

        [Command("-chan"),
            Aliases("-channel")]
        public async Task ExcludeChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use config commands.
            if (ctx.Member.GetRole().IsBotManagerOrHigher())
            {
                await ctx.Channel.TriggerTypingAsync();

                // Cancels:
                if (BotSettings.IsChannelExcluded(chan))
                {   // This channel does not exist.
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Unable to uexclude channel. This channel is already excluded..."));
                    return;
                }
                if (!ctx.Guild.Channels.Contains(chan))
                {   // This channel is not in the guild.
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Unable to exclude channel. This channel does not exist in this server..."));
                    return;
                }


                BotSettings.ExcludeChannel(chan);
                BotSettings.Save();

                await ctx.Channel.SendMessageAsync(
                    ChatObjects.GetSuccessMessage(
                        String.Format("{0} was successfully excluded!", chan.Mention)));
                await SelfAudit.LogSomething(ctx.User, @"-chan", chan.Name);
            }
        }

        [Command("chanexcludes")]
        public async Task ListExcludes(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsCHOrHigher())
            {
                await ctx.Channel.TriggerTypingAsync();

                // Cancels:

                if(BotSettings.GetExcludedChannelsCount == 0)
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetNeutralMessage(@"There are no excluded channels..."));
                    return;
                }

                // DEB!
                DiscordEmbedBuilder deb = new DiscordEmbedBuilder
                {
                    Title = "Excluded Channels"
                };

                StringBuilder sb = new StringBuilder();

                // Write out every channel being excluded into a cute little list.
                foreach (DiscordChannel chan in await BotSettings.GetExcludedChannels())
                    sb.AppendLine(chan.Mention);

                deb.Description = sb.ToString();
                deb.WithColor(DiscordColor.LightGray);
                deb.WithThumbnailUrl(ChatObjects.URL_SPEECH_BUBBLE);

                await ctx.Channel.SendMessageAsync(embed: deb);
            }
        }

        protected override void Setup(DiscordClient client) { }
    }
}
