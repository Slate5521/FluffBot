// BadWordCommands.cs
// Contains a module for configuring bad words.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Text;
using System.Threading.Tasks;
using FluffyEars.BadWords;

namespace FluffyEars.Commands
{
    class BadWordCommands : BaseModule
    {
        /// <summary>Add multiple bad words to the bad word list.</summary>
        /// <param name="words">The supposed list.</param>
        [Command("addbadwords")]
        public async Task AddBadWords(CommandContext ctx, params string[] words)
        {
            // Check if the user can use commands.
            if (BotSettings.CanUseConfigCommands(ctx.Member))
            {
                await ctx.TriggerTypingAsync();

                StringBuilder sb_fail = new StringBuilder();    // A list of words we haven't been able to add.
                StringBuilder sb_success = new StringBuilder(); // A list of words we were able to add.
                DiscordEmbedBuilder deb;

                foreach (string word in words)
                {
                    // Check if this is a bad word. If it is, we were unsuccessful at adding it to the list.
                    bool success = !BadwordSystem.IsBadWord(word);

                    if (success)
                    {
                        BadwordSystem.AddBadWord(word);
                        sb_success.Append(word + @", ");
                    }
                    else
                        sb_fail.Append(word + @", ");
                }

                // DEB!
                deb = new DiscordEmbedBuilder()
                {
                    Description = @"I attempted to add those words you gave me.",
                };


                // For each of these lists, we want to remove the last two characters, because every string will have an ", " at the end of it.
                if (sb_success.Length > 0)
                    deb.AddField("Successfully added:", sb_success.Remove(sb_success.Length - 2, 2).ToString());
                if (sb_fail.Length > 0)
                    deb.AddField("Not added:", sb_fail.Remove(sb_fail.Length - 2, 2).ToString());

                await ctx.Channel.SendMessageAsync(embed: deb.Build());

                BotSettings.Save();
            }
        }

        /// <summary>Add a single bad word to the bad word list.</summary>
        [Command("addbadword")]
        public async Task AddBadWord(CommandContext ctx, string word)
        {            
            if (BotSettings.CanUseConfigCommands(ctx.Member))
            {
                word = word.ToLower();
                await ctx.TriggerTypingAsync();

                if (BadwordSystem.IsBadWord(word))
                {
                    await ctx.Channel.SendMessageAsync("That is already a bad word.");
                }
                else
                {
                    BadwordSystem.AddBadWord(word.ToLower());

                    await ctx.Channel.SendMessageAsync("I added that bad word to the list.");
                    BotSettings.Save();
                }
            }
        }

        /// <summary>Remove a single bad word from the bad word list, if it exists.</summary>
        [Command("removebadword")]
        public async Task RemoveBadWord(CommandContext ctx, string word)
        {
            if (BotSettings.CanUseConfigCommands(ctx.Member))
            {
                await ctx.TriggerTypingAsync();

                if (BadwordSystem.IsBadWord(word))
                {
                    BadwordSystem.RemoveBadWord(word);
                    await ctx.Channel.SendMessageAsync("I removed that bad word.");
                    BotSettings.Save();

                }
                else
                {
                    await ctx.Channel.SendMessageAsync("That bad word does not exist.");
                }
            }
        }

        /// <summary>List all the bad words currently in the bad word list</summary>
        [Command("listbadwords")]
        public async Task ListBadWords(CommandContext ctx)
        {
            if (BotSettings.CanUseConfigCommands(ctx.Member))
            {
                StringBuilder sb = new StringBuilder();

                foreach (string word in BadwordSystem.GetBadWords())
                    sb.AppendLine(word);

                sb.Replace("\r\n", "\n");

                DiscordEmbedBuilder deb = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Cyan,
                    Description = sb.ToString(),
                    Title = "BAD WORDS LIST"
                };

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        protected override void Setup(DiscordClient client) { }
    }
}
