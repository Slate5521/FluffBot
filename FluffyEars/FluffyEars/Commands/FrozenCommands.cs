using DSharpPlus.CommandsNext;
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
        /// <summary>Adds a name to be recognized as a Frozen moniker.</summary>
        [Command("+name")]
        public async Task AddFrozenName(CommandContext ctx, string name)
        {   //                       Frozen                                      Emiko
            if (ctx.Member.Id.Equals(131626628211146752) || ctx.Member.Id.Equals(113829933071073287))
            {
                await ctx.TriggerTypingAsync();

                if (!BotSettings.IsFrozenName(name))
                {   // This is not an already recognized name.
                    BotSettings.AddFrozenName(name);
                    BotSettings.Save();

                    await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage(@"***HELLO FROZEN, CHAOS LORD, OR SKY, MY MASTER, I HAVE ADDED THAT TO THE GRAND LIST OF NAMES. MAY FROZEN'S OMNIPOTENCE EVER CONTINUE!***"));
                }
                else
                {   // This is a recognized name.
                    await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage(@"***HELLO FROZEN, CHAOS LORD, OR SKY, MY MASTER, I DO NOT WISH TO ALARM YOU BUT THAT APPEARS TO ALREADY BE A NAME...***"));
                }
            }
        }

        /// <summary>Removes a name from the list of recognized Frozen monikers.</summary>
        [Command("-name")]
        public async Task RemoveFrozenName(CommandContext ctx, string name)
        {   //                       Frozen                                      Emiko
            if (ctx.Member.Id.Equals(131626628211146752) || ctx.Member.Id.Equals(113829933071073287))
            {
                await ctx.TriggerTypingAsync();

                if (BotSettings.IsFrozenName(name))
                {   // This is a recognized Frozen name.
                    BotSettings.RemoveFrozenName(name);
                    BotSettings.Save();

                    await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage(@"***HELLO FROZEN, CHAOS LORD, OR SKY, MY MASTER, I HAVE REMOVED THAT FROM THE GRAND LIST OF NAMES. MAY FROZEN'S OMNIPOTENCE EVER CONTINUE!***"));
                }
                else
                {   // This is not a recognized Frozen name.
                    await ctx.Channel.SendMessageAsync(ChatObjects.GetSuccessMessage(@"***HELLO FROZEN, CHAOS LORD, OR SKY, MY MASTER, I DO NOT WISH TO ALARM YOU BUT THAT DOESN'T APPEAR TO BE A NAME...***"));
                }
            }
        }

        /// <summary>List all the recognized Frozen monikers.</summary>
        [Command("names")]
        public async Task ListFrozenNames(CommandContext ctx)
        {   //                       Frozen                                      Emiko
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

                var deb = new DiscordEmbedBuilder(
                    ChatObjects.FormatEmbedResponse
                        (
                            title: "Mention",
                            description: e.Message.Content.Length > 1500 ? $"```{e.Message.Content.Substring(0, 1500)} . . .\n**too long to preview**```" : $"```{e.Message.Content}```",
                            thumbnail: e.Message.Author.AvatarUrl,
                            color: ChatObjects.NeutralColor
                        ));

                deb.AddField(@"Mentioner", $"{e.Message.Author.Username}#{e.Message.Author.Discriminator}", true);
                deb.AddField(@"Channel", $"#{e.Message.Channel.Name}", true);
                deb.AddField(@"Link", ChatObjects.GetMessageUrl(e.Message));

                await chan.SendMessageAsync(ChatObjects.GetMention(113829933071073287), embed: deb.Build());
            }

        }
    }
}
