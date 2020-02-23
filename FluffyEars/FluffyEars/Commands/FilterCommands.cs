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
        #region Word Filter
        /// <summary>Add bad word(s) to the bad word list.</summary>
        /// <param name="words">The supposed list.</param>
        [Command("+filter")]
        public async Task FilterAddWords(CommandContext ctx, params string[] words)
        {
            // Check if the user can use commands.
            if (ctx.Member.GetRole().IsSeniorModOrHigher())
            {
                await ctx.TriggerTypingAsync();

                StringBuilder sb_fail = new StringBuilder();    // A list of words we haven't been able to add.
                StringBuilder sb_success = new StringBuilder(); // A list of words we were able to add.
                DiscordEmbedBuilder deb;

                foreach (string word in words)
                {
                    // Check if this is in the filter list or exclude list. If it is, we were unsuccessful at adding it to the list.
                    bool success = !FilterSystem.IsWord(word) && !Excludes.IsExcluded(word);

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
                    Description = ChatObjects.GetNeutralMessage(@"I attempted to add those words you gave me."),
                    Color = DiscordColor.LightGray
                };

                deb.WithThumbnailUrl(ChatObjects.URL_FILTER_ADD);

                // For each of these lists, we want to remove the last two characters, because every string will have an ", " at the end of it.
                if (sb_success.Length > 0)
                    deb.AddField(@"Successfully added:", sb_success.Remove(sb_success.Length - 2, 2).ToString());
                if (sb_fail.Length > 0)
                    deb.AddField(@"Not added:", sb_fail.Remove(sb_fail.Length - 2, 2).ToString());

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
                await SelfAudit.LogSomething(ctx.User, @"+filter", String.Join(' ', words));
                
                FilterSystem.Save();
            }
        }

        /// <summary>Remove a single bad word from the bad word list, if it exists.</summary>
        [Command("-filter")]
        public async Task RemoveFilterWords(CommandContext ctx, params string[] words)
        {
            // Check if the user can use commands.
            if (ctx.Member.GetRole().IsSeniorModOrHigher())
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
                    Description = ChatObjects.GetNeutralMessage(@"I attempted to remove those words you gave me."),
                    Color = DiscordColor.LightGray
                };

                deb.WithThumbnailUrl(ChatObjects.URL_FILTER_SUB);

                // For each of these lists, we want to remove the last two characters, because every string will have an ", " at the end of it.
                if (sb_success.Length > 0)
                    deb.AddField("Successfully removed:", sb_success.Remove(sb_success.Length - 2, 2).ToString());
                if (sb_fail.Length > 0)
                    deb.AddField("Not removed:", sb_fail.Remove(sb_fail.Length - 2, 2).ToString());

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
                await SelfAudit.LogSomething(ctx.User, @"-filter", String.Join(' ', words));

                FilterSystem.Save();
            }
        }

        /// <summary>List all the bad words currently in the bad word list</summary>
        [Command("filterlist")]
        public async Task ListFilterWords(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsModOrHigher())
            {
                StringBuilder sb = new StringBuilder();

                foreach (string word in FilterSystem.GetWords())
                    sb.AppendLine(word);

                DiscordEmbedBuilder deb = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.LightGray,
                    Description = sb.ToString(),
                    Title = "FILTER WORD LIST",
                    ThumbnailUrl = ChatObjects.URL_FILTER_GENERIC
                };

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        [Command("+filterexclude")]
        public async Task FilterExclude(CommandContext ctx, string word)
        {
            if (ctx.Member.GetRole().IsSeniorModOrHigher())
            {
                await ctx.TriggerTypingAsync();

                // Cancels:
                if (FilterSystem.IsWord(word))
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Cannot add that word. It's a filter word..."));
                    return;
                }
                if (Excludes.IsExcluded(word))
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Cannot add that word. It's already excluded..."));
                    return;
                }

                Excludes.AddWord(word);
                Excludes.Save();

                await ctx.Channel.SendMessageAsync(
                    ChatObjects.GetSuccessMessage(
                        String.Format("I excluded the word {0}!", word)));
                await SelfAudit.LogSomething(ctx.User, @"+filterexclude", String.Join(' ', word));

            }
        }

        [Command("-filterexclude")]
        public async Task FilterInclude(CommandContext ctx, string word)
        {
            if (ctx.Member.GetRole().IsSeniorModOrHigher())
            {
                await ctx.TriggerTypingAsync();

                if (!Excludes.IsExcluded(word))
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Cannot un-exclude that word. It's not already excluded."));
                }
                else
                {

                    Excludes.RemoveWord(word);
                    Excludes.Save();

                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetSuccessMessage(@"I removed that word from exclusion."));
                    await SelfAudit.LogSomething(ctx.User, @"-filterexclude", String.Join(' ', word));

                }
            }
        }

        [Command("filterexcludes")]
        public async Task ListFilterWordExcludes(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsModOrHigher())
            {
                StringBuilder sb = new StringBuilder();

                // Cancels:
                if (Excludes.GetWordCount() == 0)
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetNeutralMessage(@"There are no filter excludes..."));
                    return;
                }

                foreach (string word in Excludes.GetWords())
                    sb.AppendLine(word);

                DiscordEmbedBuilder deb = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.LightGray,
                    Description = sb.ToString(),
                    Title = "FILTER LIST EXCLUDES"
                };

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        #endregion Word Filter
        protected override void Setup(DiscordClient client) { }
    }
}
