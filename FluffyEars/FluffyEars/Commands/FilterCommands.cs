// FilterCommands.cs
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
    class FilterCommands : BaseModule
    {
        /// <summary>Add bad word(s) to the bad word list.</summary>
        /// <param name="words">The supposed list.</param>
        [Command("+filter"), 
            Description("[SEN.MOD+] Adds a single word or multiple words to the filter list. Accepts RegEx. \nUsage: +filter word1 word1 word2 ... wordn")]
        public async Task FilterAddWords(CommandContext ctx, params string[] words)
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
                    // Check if this is in the filter list or exclude list. If it is, we were unsuccessful at adding it to the list.
                    bool success = !FilterSystem.IsWord(word) || !Excludes.IsExcluded(word);

                    if (success)
                    {
                        FilterSystem.AddWord(word);
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
                    deb.AddField(@"Successfully added:", sb_success.Remove(sb_success.Length - 2, 2).ToString());
                if (sb_fail.Length > 0)
                    deb.AddField(@"Not added (already in the filter system):", sb_fail.Remove(sb_fail.Length - 2, 2).ToString());

                await ctx.Channel.SendMessageAsync(embed: deb.Build());

                FilterSystem.Save();
            }
        }

        /// <summary>Remove a single bad word from the bad word list, if it exists.</summary>
        [Command("-filter"),
            Description("[SEN.MOD+] Removes a single word or multiple words from the filter list.\nUsage: -filter word word1 word2 ... wordn")]
        public async Task RemoveFilterWords(CommandContext ctx, params string[] words)
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
                    // Check if this is already in the filter list. If it is not, we were unsuccessful at adding it to the list.
                    bool success = FilterSystem.IsWord(word);

                    if (success)
                    {
                        FilterSystem.RemoveWord(word);
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

                FilterSystem.Save();
            }
        }

        /// <summary>List all the bad words currently in the bad word list</summary>
        [Command("listfilter"),
            Description("[MOD+] Lists all the bad words currently being watched for.")]
        public async Task ListFilterWords(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsModOrHigher())
            {
                StringBuilder sb = new StringBuilder();

                foreach (string word in FilterSystem.GetWords())
                    sb.AppendLine(word);

                sb.Replace("\r\n", "\n");

                DiscordEmbedBuilder deb = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Cyan,
                    Description = sb.ToString(),
                    Title = "FILTER LIST"
                };

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        [Command("+filterexclude"),
            Description("[SEN.MOD+] Excludes this word from triggering the filter system.")]
        public async Task FilterExclude(CommandContext ctx, string word)
        {
            if(ctx.Member.GetRole().IsSeniorModOrHigher())
            {
                await ctx.TriggerTypingAsync();

                if (FilterSystem.IsWord(word) || Excludes.IsExcluded(word))
                    await ctx.Channel.SendMessageAsync("Cannot add that word. It's already excluded or a filter system trigger.");
                else
                {
                    Excludes.AddWord(word);
                    await ctx.Channel.SendMessageAsync("I excluded that word.");
                }
            }
        }

        [Command("-filterexclude"),
            Description("[SEN.MOD+] Removes this word from the exclusion list, if it exists, allowing it to trigger the filter system.")]
        public async Task FilterInclude(CommandContext ctx, string word)
        {
            if (ctx.Member.GetRole().IsSeniorModOrHigher())
            {
                await ctx.TriggerTypingAsync();

                if (!Excludes.IsExcluded(word))
                    await ctx.Channel.SendMessageAsync("Cannot unexclude that word. It's not in the exclude list.");
                else
                {
                    Excludes.RemoveWord(word);
                    await ctx.Channel.SendMessageAsync("I removed that word from exclusion.");
                }
            }
        }

        [Command("listfilterexcludes"),
            Description("[MOD+] Lists all the bad words currently being excluded for.")]
        public async Task ListFilterWordExcludes(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsModOrHigher())
            {
                StringBuilder sb = new StringBuilder();

                foreach (string word in Excludes.GetWords())
                    sb.AppendLine(word);

                sb.Replace("\r\n", "\n");

                DiscordEmbedBuilder deb = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Cyan,
                    Description = sb.ToString(),
                    Title = "FILTER LIST EXCLUDES"
                };

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        protected override void Setup(DiscordClient client) { }
    }
}
