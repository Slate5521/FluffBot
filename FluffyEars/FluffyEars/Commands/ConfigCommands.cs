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
        [Command("setfilterchan"), 
            Aliases("setfilterchannel", "setfilter"),
            Description("[OWNER/ADMIN/BOT.BOI] Sets the channel the bot will record bad words into.\nUsage: setfilterchan #DiscordChannel")]
        public async Task SetFilterChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use these sorts of commands.
            if (ctx.Member.GetRole().IsBotManagerOrHigher())
            {
                await ctx.TriggerTypingAsync(); 

                // Check if the channel is in the guild
                if (ctx.Guild.Channels.ToArray().Contains(chan))
                {
                    // Update the settings.
                    BotSettings.FilterChannelId = chan.Id;
                    BotSettings.Save();

                    await ctx.Channel.SendMessageAsync("Filter channel set.");
                    await SelfAudit.LogSomething(ctx.User, @"filter channel", chan.Name);
                }
                else await ctx.Channel.SendMessageAsync("Unable to set channel. Does not exist or is not in this guild.");
            }
        }

        [Command("+chan"),
            Aliases("+channel"),
            Description("[OWNER/ADMIN/BOT.BOI] Un-exclude a channel from bad word searching.\nUsage: +chan #DiscordChannel")]
        public async Task IncludeChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use config commands.
            if (ctx.Member.GetRole().IsBotManagerOrHigher())
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
                await SelfAudit.LogSomething(ctx.User, @"+chan", chan.Name);
            }
        }

        [Command("-chan"),
            Aliases("-channel"),
            Description("[OWNER/ADMIN/BOT.BOI] Exclude a channel from bad word searching.\nUsage: -chan #DiscordChannel")]
        public async Task ExcludeChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use config commands.
            if (ctx.Member.GetRole().IsBotManagerOrHigher())
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
                await SelfAudit.LogSomething(ctx.User, @"-chan", chan.Name);
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
