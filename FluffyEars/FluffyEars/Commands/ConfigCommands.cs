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
    class ConfigCommands : BaseCommandModule
    {
        public ConfigCommands() { }

        #region Filter Config Commands

        [Command("setfilterchan")]
        public async Task SetFilterChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use these sorts of commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                DiscordEmbed embedResponse;
                bool success = true;

                await ctx.TriggerTypingAsync();

                // Cancels:
                if (BotSettings.FilterChannelId == chan.Id)
                {   // Trying to set the channel to itself.
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Filter channel not set",
                            description: ChatObjects.GetErrMessage($"The channel {chan.Mention} is already the filter channel..."),
                            color: ChatObjects.ErrColor
                        );

                    success = false;

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }

                if (!ctx.Guild.Channels.ContainsKey(chan.Id))
                {   // Channel does not exist.

                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Filter channel not set",
                            description: ChatObjects.GetErrMessage($"Supposed channel does not exist or is not in this guild..."),
                            color: ChatObjects.ErrColor
                        );

                    success = false;

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }

                // Update the settings.
                BotSettings.FilterChannelId = chan.Id;
                BotSettings.Save();

                if (success)
                {
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Filter channel set",
                            description: ChatObjects.GetSuccessMessage($"Filter channel set to {chan.Mention}!"),
                            color: ChatObjects.SuccessColor
                        );

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }
            }
        }

        [Command("+chan"), Aliases("+channel")]
        public async Task IncludeChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use config commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                DiscordEmbed embedResponse;
                bool success = true;

                await ctx.Channel.TriggerTypingAsync();

                if (!BotSettings.IsChannelExcluded(chan))
                {   // This channel is excluded already.
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Channel not un-excluded",
                            description: ChatObjects.GetErrMessage($"The channel {chan.Mention} is not already excluded..."),
                            color: ChatObjects.ErrColor
                        );

                    success = false;

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }

                if (!ctx.Guild.Channels.ContainsKey(chan.Id))
                {   // This channel is not in the guild.

                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Channel not un-excluded",
                            description: ChatObjects.GetErrMessage($"Supposed channel does not exist or is not in this guild..."),
                            color: ChatObjects.ErrColor
                        );

                    success = false;

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }

                BotSettings.IncludeChannel(chan);
                BotSettings.Save();

                if (success)
                {
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Channel un-excluded",
                            description: ChatObjects.GetSuccessMessage($"The channel {chan.Mention} was successfully un-excluded!"),
                            color: ChatObjects.SuccessColor
                        );

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }
            }
        }

        [Command("-chan"), Aliases("-channel")]
        public async Task ExcludeChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use config commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                DiscordEmbed embedResponse;
                bool success = true;

                await ctx.Channel.TriggerTypingAsync();

                if (BotSettings.IsChannelExcluded(chan))
                {   // This channel is exlcuded alraedy.
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Channel not excluded",
                            description: ChatObjects.GetErrMessage($"The channel {chan.Mention} is already excluded..."),
                            color: ChatObjects.ErrColor
                        );

                    success = false;

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }

                if (!ctx.Guild.Channels.ContainsKey(chan.Id))
                {   // This channel is not in the guild.

                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Channel not excluded",
                            description: ChatObjects.GetErrMessage($"Supposed channel does not exist or is not in this guild..."),
                            color: ChatObjects.ErrColor
                        );

                    success = false;

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }

                BotSettings.ExcludeChannel(chan);
                BotSettings.Save();

                if (success)
                {
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Channel excluded",
                            description: ChatObjects.GetSuccessMessage($"The channel {chan.Mention} was successfully excluded!"),
                            color: ChatObjects.SuccessColor
                        );

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }
            }
        }

        [Command("chanexcludes")]
        public async Task ListExcludes(CommandContext ctx)
        {
            // Check if the user can use config commands.
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

                DiscordEmbed embedResponse;

                if (BotSettings.GetExcludedChannelsCount == 0)
                {   // There are no excludes.
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Excluded channels",
                            description: ChatObjects.GetNeutralMessage(@"There are no excluded channels."),
                            color: ChatObjects.NeutralColor
                        );

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    // Write out every channel being excluded into a cute little list.
                    foreach (DiscordChannel chan in await BotSettings.GetExcludedChannels())
                        sb.AppendLine(chan.Mention);

                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Excluded channels",
                            description: ChatObjects.GetNeutralMessage($"Here's a list of excluded channels:\n{sb.ToString()}"),
                            color: ChatObjects.NeutralColor
                        );

                    await ctx.Channel.SendMessageAsync(embed: embedResponse);
                }
            }
        }

        #endregion Filter Config Commands

        [Command("startmessageenabled")]
        public async Task StartMessageEnabled(CommandContext ctx, bool enabled)
        {
            // Check if the user can use config commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                await ctx.TriggerTypingAsync();

                DiscordEmbed embedResponse;

                BotSettings.StartMessageEnabled = enabled;
                BotSettings.Save();

                var stringBuilder = new StringBuilder();
                stringBuilder.Append(@"I have ");

                if (enabled)
                {
                    stringBuilder.Append(@"enabled start messaging! :D");
                }
                else
                {
                    stringBuilder.Append(@"disabled start messaging... ;-;");
                }

                embedResponse = ChatObjects.FormatEmbedResponse
                    (
                        title: @"Excluded channels",
                        description: ChatObjects.GetNeutralMessage(stringBuilder.ToString()),
                        color: ChatObjects.NeutralColor
                    );

                await ctx.Channel.SendMessageAsync(embed: embedResponse);
            }
        }

        #region Rimboard

        [Command("rimboardemoji")]
        public async Task SetRimboardEmoji(CommandContext ctx, DiscordEmoji emoji)
        {
            // Check if the user can use config commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                BotSettings.RimboardEmoji = emoji.GetDiscordName();
                BotSettings.Save();

                await ctx.Channel.SendMessageAsync(embed: ChatObjects.FormatEmbedResponse
                    (
                        title: @"Rimboard Emoji",
                        description: ChatObjects.GetSuccessMessage($"I set the Rimboard emoji to {Formatter.Emoji(emoji)}"),
                        color: ChatObjects.SuccessColor
                    ));
            }
        }

        [Command("rimboardchan")]
        public async Task SetRimboardChannel(CommandContext ctx, DiscordChannel channel)
        {
            // Check if the user can use config commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                BotSettings.RimboardChannelId = channel.Id;
                BotSettings.Save();

                await ctx.Channel.SendMessageAsync(embed: ChatObjects.FormatEmbedResponse
                    (
                        title: @"Rimboard",
                        description: ChatObjects.GetSuccessMessage($"I set the Rimboard channel to {channel.Mention}"),
                        color: ChatObjects.SuccessColor
                    ));
            }
        }

        [Command("pincount")]
        public async Task SetPinCount(CommandContext ctx, uint count)
        {
            // Check if the user can use config commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                BotSettings.RimboardThreshold = (int)count;
                BotSettings.Save();

                await ctx.Channel.SendMessageAsync(embed: ChatObjects.FormatEmbedResponse
                    (
                        title: @"Rimboard Count",
                        description: ChatObjects.GetSuccessMessage($"I set the number of reacts required to {count}"),
                        color: ChatObjects.SuccessColor
                    ));
            }
        }

        #endregion Rimboard
    }
}

