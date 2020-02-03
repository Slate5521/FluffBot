// ConfigCommands.cs
// Contains a module for modifying the general config.

using System;
using System.Collections.Generic;
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
        [Command("setfilterchan"), 
            Aliases("setfilterchannel", "setfilter"),
            Description("[OWNER/ADMIN/BOT.BOI] Sets the channel the bot will record bad words into.\nUsage: setfilterchan #DiscordChannel")]
        public async Task SetFilterChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use these sorts of commands.
            if (BotSettings.CanUseConfigCommands(ctx.Member))
            {
                await ctx.TriggerTypingAsync();

                // Update the settings.
                BotSettings.FilterChannelId = chan.Id;
                BotSettings.Save();

                await ctx.Channel.SendMessageAsync("Audit channel set.");
            }
        }

        [Command("+whitelist"),
            Description("[OWNER] Whitelist a specific user.\nUsage: +whitelist @DiscordUser")]
        public async Task WhitelistUser(CommandContext ctx, DiscordMember user)
        {
            // Owner only.
            if (ctx.Member.IsOwner)
            {
                string response;
                await ctx.TriggerTypingAsync();

                // Only add the user to white list if he or she is not on it.
                if (!BotSettings.IsUserOnWhitelist(user))
                {
                    BotSettings.AddUserToWhitelist(user);
                    response = "User added to whitelist.";
                }
                else response = "User already on whitelist.";

                await ctx.Channel.SendMessageAsync(response);
            }
        }

        [Command("-whitelist"),
            Description("[OWNER] Remove a specific user from the whitelist.\nUsage: -whitelist @DiscordUser")]
        public async Task BlacklistUser(CommandContext ctx, DiscordMember user)
        {
            // Owner only.
            if (ctx.Member.IsOwner)
            {
                string response;
                await ctx.TriggerTypingAsync();

                // Only remove the user from the whitelist if he or she is on it.
                if (BotSettings.IsUserOnWhitelist(user))
                {
                    BotSettings.RemoveUserFromWhitelist(user);
                    response = "User removed from whitelist.";
                }
                else response = "User not on whitelist.";

                await ctx.Channel.SendMessageAsync(response);
            }
        }

        [Command("+chan"),
            Aliases("+channel"),
            Description("[OWNER/ADMIN/BOT.BOI] Un-exclude a channel from bad word searching.\nUsage: +chan #DiscordChannel")]
        public async Task IncludeChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use config commands.
            if (BotSettings.CanUseConfigCommands(ctx.Member))
            {
                string response = "Unknown error.";
                await ctx.Channel.TriggerTypingAsync();

                // Only enter scope if (a) channel is currently exluced && (b) the channel belongs to Rimworld.
                if (BotSettings.IsChannelExcluded(chan) && (chan.Guild == ctx.Guild))
                {
                    BotSettings.IncludeChannel(chan);
                    response = "Channel successfully removed from exclude list.";
                }
                else if (!BotSettings.IsChannelExcluded(chan))
                    response = "Channel not in exclude list.";

                await ctx.Channel.SendMessageAsync(response);
            }
        }

        [Command("-chan"),
            Aliases("-channel"),
            Description("[OWNER/ADMIN/BOT.BOI] Exclude a channel from bad word searching.\nUsage: -chan #DiscordChannel")]
        public async Task ExcludeChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use config commands.
            if (BotSettings.CanUseConfigCommands(ctx.Member))
            {
                string response = "Unknown error.";
                await ctx.Channel.TriggerTypingAsync();

                // Only enter scope if (a) channel is NOT already excluded && (b) the channel belongs to Rimworld. 
                if (!BotSettings.IsChannelExcluded(chan) && (chan.Guild == ctx.Guild))
                {
                    BotSettings.ExcludeChannel(chan);
                    response = "Channel successfully excluded.";
                }
                else if (BotSettings.IsChannelExcluded(chan))
                    response = "Channel already excluded.";

                await ctx.Channel.SendMessageAsync(response);
            }
        }
        [Command("listexcludes"),
            Description("List excluded channels.")]
        public async Task ListExcludes(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsCHOrHigher())
            {
                await ctx.Channel.TriggerTypingAsync();

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

                await ctx.Channel.SendMessageAsync(embed: deb);
            }
        }

        protected override void Setup(DiscordClient client) { }
    }
}
