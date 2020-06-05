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
    class FilterCommands : BaseCommandModule
    {
        public FilterCommands() { }

        #region Word Filter
        [Command("+filter")]
        public async Task FilterAddMask(CommandContext ctx, params string[] args)
        {
            // Check if the user can use commands.
            if (ctx.Member.GetHighestRole().IsSeniorModOrHigher() && args.Length > 0)
            {
                await ctx.TriggerTypingAsync();

                string mask = ParamsToString(args);

                // DEB!
                DiscordEmbedBuilder deb = new DiscordEmbedBuilder();

                bool success = !FilterSystem.IsMask(mask) && !Excludes.IsExcluded(mask);

                if (success)
                {
                    FilterSystem.AddMask(mask);
                    FilterSystem.Save();

                    deb.WithColor(DiscordColor.Green);
                    deb.WithDescription(ChatObjects.GetSuccessMessage(@"I was able to add the mask you gave me!"));
                }
                else
                {
                    deb.WithColor(DiscordColor.Red);
                    deb.WithDescription(ChatObjects.GetErrMessage(@"I was unable able to add the mask you gave me... It already exists..."));
                }

                deb.WithThumbnail(ChatObjects.URL_FILTER_ADD);
                deb.AddField(@"Mask", $"`{mask}`");

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        /// <summary>Remove a single bad word from the bad word list, if it exists.</summary>
        [Command("-filter")]
        public async Task RemoveFilterMask(CommandContext ctx, params string[] args)
        {
            // Check if the user can use commands.
            if (ctx.Member.GetHighestRole().IsSeniorModOrHigher() && args.Length > 0)
            {
                await ctx.TriggerTypingAsync();

                string mask = ParamsToString(args);

                // DEB!
                DiscordEmbedBuilder deb = new DiscordEmbedBuilder();

                if (FilterSystem.IsMask(mask))
                {
                    FilterSystem.RemoveMask(mask);
                    FilterSystem.Save();

                    deb.WithColor(DiscordColor.Green);
                    deb.WithDescription(ChatObjects.GetSuccessMessage(@"I was able to remove the mask you gave me!"));
                }
                else
                {
                    deb.WithColor(DiscordColor.Red);
                    deb.WithDescription(ChatObjects.GetErrMessage(@"I was unable able to remove the mask you gave me... It doesn't exist..."));
                }

                deb.WithThumbnail(ChatObjects.URL_FILTER_ADD);
                deb.AddField(@"Mask", $"`{mask}`");

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        /// <summary>List all the bad words currently in the bad word list</summary>
        [Command("filterlist")]
        public async Task ListFilterMasks(CommandContext ctx)
        {
            if (ctx.Member.GetHighestRole().IsModOrHigher())
            {
                StringBuilder sb = new StringBuilder();

                foreach (string mask in FilterSystem.GetMasks())
                    sb.AppendLine(mask);

                DiscordEmbedBuilder deb = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.LightGray,
                    Description = sb.ToString(),
                    Title = "FILTER MASK LIST",
                };

                deb.WithThumbnail(ChatObjects.URL_FILTER_GENERIC);

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        [Command("+filterexclude")]
        public async Task FilterExclude(CommandContext ctx, params string[] args)
        {
            if (ctx.Member.GetHighestRole().IsSeniorModOrHigher() && args.Length > 0)
            {
                await ctx.TriggerTypingAsync();

                string phrase = ParamsToString(args);

                // Cancels:
                if (FilterSystem.IsMask(phrase))
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Cannot add that phrase. It's a filter word..."));
                    return;
                }
                if (Excludes.IsExcluded(phrase))
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Cannot add that phrase. It's already excluded..."));
                    return;
                }

                Excludes.AddPhrase(phrase);
                Excludes.Save();

                await ctx.Channel.SendMessageAsync(
                    ChatObjects.GetSuccessMessage(
                        String.Format("I excluded the phrase {0}!", phrase)));

            }
        }

        [Command("-filterexclude")]
        public async Task FilterRemoveExclude(CommandContext ctx, params string[] args)
        {
            if (ctx.Member.GetHighestRole().IsSeniorModOrHigher() && args.Length > 0)
            {
                await ctx.TriggerTypingAsync();

                string phrase = ParamsToString(args);

                if (!Excludes.IsExcluded(phrase))
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Cannot un-exclude that phrase. It's not already excluded..."));
                }
                else
                {
                    Excludes.RemovePhrase(phrase);
                    Excludes.Save();

                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetSuccessMessage(@"I removed that phrase from exclusion!"));
                }
            }
        }

        [Command("filterexcludes")]
        public async Task ListFilterExcludes(CommandContext ctx)
        {
            if (ctx.Member.GetHighestRole().IsModOrHigher())
            {
                StringBuilder sb = new StringBuilder();

                // Cancels:
                if (Excludes.GetPhraseCount() == 0)
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetNeutralMessage(@"There are no filter excludes..."));
                    return;
                }

                foreach (string word in Excludes.GetPhrases())
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

        private static string ParamsToString(string[] paramsStr)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendJoin(' ', paramsStr);

            return sb.ToString();
        }

        #endregion Word Filter
    }
}
