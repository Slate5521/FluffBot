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
        /// <summary>Add bad word(s) to the bad word list.</summary>
        /// <param name="words">The supposed list.</param>
        [Command("+badwords"), 
            Aliases("+badword"),
            Description("[SEN.MOD+] Adds a single bad word or multiple bad words to the bad word list.\nUsage: +badword badword badword1 badword2 ... badwordn")]
        public async Task AddBadWords(CommandContext ctx, params string[] words)
        {
            // Check if the user can use commands.
            if (ctx.Member.GetRole().IsModOrHigher())
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

                BadwordSystem.Save();
            }
        }

        /// <summary>Remove a single bad word from the bad word list, if it exists.</summary>
        [Command("-badwords"),
            Aliases("-badword"),
            Description("[SEN.MOD+] Removes a single or multiple bad words from the bad word list.\nUsage: -badword badword badword1 badword2 ... badwordn")]
        public async Task RemoveBadWords(CommandContext ctx, params string[] words)
        {
            // Check if the user can use commands.
            if (ctx.Member.GetRole().IsModOrHigher())
            {
                await ctx.TriggerTypingAsync();

                StringBuilder sb_fail = new StringBuilder();    // A list of words we haven't been able to add.
                StringBuilder sb_success = new StringBuilder(); // A list of words we were able to add.
                DiscordEmbedBuilder deb;

                foreach (string word in words)
                {
                    // Check if this is a bad word. If it is not, we were unsuccessful at adding it to the list.
                    bool success = BadwordSystem.IsBadWord(word);

                    if (success)
                    {
                        BadwordSystem.RemoveBadWord(word);
                        sb_success.Append(word + @", ");
                    }
                    else
                        sb_fail.Append(word + @", ");
                }

                // DEB!
                deb = new DiscordEmbedBuilder()
                {
                    Description = @"I attempted to remove those words you gave me.",
                };


                // For each of these lists, we want to remove the last two characters, because every string will have an ", " at the end of it.
                if (sb_success.Length > 0)
                    deb.AddField("Successfully removed:", sb_success.Remove(sb_success.Length - 2, 2).ToString());
                if (sb_fail.Length > 0)
                    deb.AddField("Not removed:", sb_fail.Remove(sb_fail.Length - 2, 2).ToString());

                await ctx.Channel.SendMessageAsync(embed: deb.Build());

                BadwordSystem.Save();
            }
        }

        /// <summary>List all the bad words currently in the bad word list</summary>
        [Command("listbadwords"),
            Description("[MOD+] Lists all the bad words currently being watched for.")]
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
