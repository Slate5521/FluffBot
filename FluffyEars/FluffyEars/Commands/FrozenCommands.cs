﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//|￣￣￣￣￣￣￣￣|
//|     FROZEN    |
//|    MADE ME    | 
//|     DO IT     |
//| ＿＿＿＿＿＿＿| 
//(\__/) || 
//(•ㅅ•) || 
/// 　 づ
/// 　 /
/// 　 I SWEAR!

namespace FluffyEars.Commands
{
    public class FrozenCommands : BaseCommandModule
    {
        [Command("+name")]
        public async Task AddFrozenName(CommandContext ctx, string name)
        {
            if (ctx.Member.Id.Equals(131626628211146752) || ctx.Member.Id.Equals(113829933071073287))
            {
                await ctx.TriggerTypingAsync();

                if(!BotSettings.IsFrozenName(name))
                {
                    BotSettings.AddFrozenName(name);
                    BotSettings.Save();

                    await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage(@"***HELLO FROZEN, CHAOS LORD, OR SKY, MY MASTER, I HAVE ADDED THAT TO THE GRAND LIST OF NAMES. MAY FROZEN'S OMNIPOTENCE EVER CONTINUE!***"));
                } else
                    await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage(@"***HELLO FROZEN, CHAOS LORD, OR SKY, MY MASTER, I DO NOT WISH TO ALARM YOU BUT THAT APPEARS TO ALREADY BE A NAME...***"));
                
            }
        }

        [Command("-name")]
        public async Task RemoveFrozenName(CommandContext ctx, string name)
        {
            if (ctx.Member.Id.Equals(131626628211146752) || ctx.Member.Id.Equals(113829933071073287))
            {
                await ctx.TriggerTypingAsync();

                if (BotSettings.IsFrozenName(name))
                {
                    BotSettings.RemoveFrozenName(name);
                    BotSettings.Save();

                    await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage(@"***HELLO FROZEN, CHAOS LORD, OR SKY, MY MASTER, I HAVE REMOVED THAT FROM THE GRAND LIST OF NAMES. MAY FROZEN'S OMNIPOTENCE EVER CONTINUE!***"));
                } else
                    await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage(@"***HELLO FROZEN, CHAOS LORD, OR SKY, MY MASTER, I DO NOT WISH TO ALARM YOU BUT THAT DOESN'T APPEAR TO BE A NAME...***"));
            }
        }

        [Command("names")]
        public async Task ListFrozenNames(CommandContext ctx)
        {
            if (ctx.Member.Id.Equals(131626628211146752) || ctx.Member.Id.Equals(113829933071073287))
            {
                await ctx.TriggerTypingAsync();

                string names = new StringBuilder().AppendJoin(", ", BotSettings.GetFrozenNames()).ToString();

                await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage($"***AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\n{names}***"));
            }
        }

        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            // AND WITH THIS, FROZEN'S OMNIPOTENCE IS ASSURED!

            if (e.Author.Id != 669347771312111619 && BotSettings.GetFrozenNames().Any(e.Message.Content.ToLower().Contains) && !e.Author.IsBot)
            {
                DiscordChannel chan = await Bot.BotClient.GetChannelAsync(679933620034600960);
                await chan.SendMessageAsync(String.Format("<@113829933071073287> Someone mentioned you!\n{0}",
                    String.Format("https://discordapp.com/channels/{0}/{1}/{2}", e.Channel.GuildId, e.Channel.Id, e.Message.Id)));
                await chan.SendMessageAsync(e.Message.Content);
            }

        }
    }
}