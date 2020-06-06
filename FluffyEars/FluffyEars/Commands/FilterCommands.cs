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
            if (!ctx.Member.GetHighestRole().IsSeniorModOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                       (
                           requiredRole: Role.SeniorModerator,
                           command: ctx.Command.Name,
                           channel: ctx.Channel,
                           caller: ctx.Member
                       );
            }
            else
            {
                if (args.Length > 0)
                {
                    await ctx.TriggerTypingAsync();

                    DiscordEmbedBuilder deb;

                    string mask = ParamsToString(args);

                    if (!FilterSystem.IsMask(mask) && !Excludes.IsExcluded(mask))
                    {
                        FilterSystem.AddMask(mask);
                        FilterSystem.Save();

                        deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: @"Add Mask",
                            description: ChatObjects.GetNeutralMessage(@"I was able to add the mask you gave me!"),
                            color: ChatObjects.SuccessColor,
                            thumbnail: ChatObjects.URL_FILTER_ADD
                        ));
                    }
                    else
                    {
                        deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: @"Unable to Add Mask",
                            description: ChatObjects.GetNeutralMessage(@"I was unable to add the mask you gave me. It's already a mask or a filter exclude..."),
                            color: ChatObjects.ErrColor,
                            thumbnail: ChatObjects.URL_FILTER_ADD
                        ));
                    }

                    deb.AddField(@"Mask", $"`{mask}`");

                    await ctx.Channel.SendMessageAsync(embed: deb.Build());
                } // end if
            } // end else
        } // end method

        /// <summary>Remove a single bad word from the bad word list, if it exists.</summary>
        [Command("-filter")]
        public async Task RemoveFilterMask(CommandContext ctx, params string[] args)
        {
            // Check if the user can use commands.
            if (!ctx.Member.GetHighestRole().IsSeniorModOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                       (
                           requiredRole: Role.SeniorModerator,
                           command: ctx.Command.Name,
                           channel: ctx.Channel,
                           caller: ctx.Member
                       );
            }
            else
            {
                if (args.Length > 0)
                {
                    await ctx.TriggerTypingAsync();

                    string mask = ParamsToString(args);

                    DiscordEmbedBuilder deb;

                    if (FilterSystem.IsMask(mask))
                    {
                        FilterSystem.RemoveMask(mask);
                        FilterSystem.Save();

                        deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: @"Add Mask",
                            description: ChatObjects.GetNeutralMessage(@"I was able to remove the mask you gave me!"),
                            color: ChatObjects.SuccessColor,
                            thumbnail: ChatObjects.URL_FILTER_SUB
                        ));
                    }
                    else
                    {
                        deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: @"Unable to Add Mask",
                            description: ChatObjects.GetNeutralMessage(@"I was unable to add the mask you gave me. It doesn't exist..."),
                            color: ChatObjects.ErrColor,
                            thumbnail: ChatObjects.URL_FILTER_SUB
                        ));
                    }

                    deb.AddField(@"Mask", $"`{mask}`");

                    await ctx.Channel.SendMessageAsync(embed: deb.Build());
                } // end if
            } // end else
        } // end method

        /// <summary>List all the bad words currently in the bad word list</summary>
        [Command("filterlist")]
        public async Task ListFilterMasks(CommandContext ctx)
        {
            if (!ctx.Member.GetHighestRole().IsModOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                       (
                           requiredRole: Role.Moderator,
                           command: ctx.Command.Name,
                           channel: ctx.Channel,
                           caller: ctx.Member
                       );
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (string mask in FilterSystem.GetMasks())
                    sb.AppendLine($"`{mask}`");

                await ctx.Channel.SendMessageAsync(
                    embed:

                    ChatObjects.FormatEmbedResponse
                    (
                        title: "List of Masks",
                        description: sb.ToString(),
                        color: ChatObjects.NeutralColor,
                        thumbnail: ChatObjects.URL_FILTER_GENERIC
                    ));
            }
        }

        [Command("+filterexclude")]
        public async Task FilterExclude(CommandContext ctx, params string[] args)
        {
            if (!ctx.Member.GetHighestRole().IsSeniorModOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                       (
                           requiredRole: Role.SeniorModerator,
                           command: ctx.Command.Name,
                           channel: ctx.Channel,
                           caller: ctx.Member
                       );
            }
            else
            {
                await ctx.TriggerTypingAsync();

                string phrase = ParamsToString(args);
                DiscordEmbedBuilder deb;

                // Cancels:
                if (FilterSystem.IsMask(phrase) || Excludes.IsExcluded(phrase))
                {
                    deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                    (
                        title: @"Unable to Add Exclude",
                        description: ChatObjects.GetErrMessage(@"I was unable to exclude that phrase. It's either excluded already or a mask..."),
                        color: ChatObjects.ErrColor,
                        thumbnail: ChatObjects.URL_FILTER_ADD
                    ));
                }
                else
                {
                    Excludes.AddPhrase(phrase);
                    Excludes.Save();

                    deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                    (
                        title: @"Add Exclude",
                        description: ChatObjects.GetSuccessMessage(@"I was able to exclude that phrase!"),
                        color: ChatObjects.SuccessColor,
                        thumbnail: ChatObjects.URL_FILTER_ADD
                    ));
                }

                deb.AddField(@"Phrase", phrase);

                await ctx.Channel.SendMessageAsync(embed: deb.Build());
            }
        }

        [Command("-filterexclude")]
        public async Task FilterRemoveExclude(CommandContext ctx, params string[] args)
        {
            if (!ctx.Member.GetHighestRole().IsSeniorModOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                       (
                           requiredRole: Role.SeniorModerator,
                           command: ctx.Command.Name,
                           channel: ctx.Channel,
                           caller: ctx.Member
                       );
            }
            else 
            {
                if (args.Length > 0)
                {
                    await ctx.TriggerTypingAsync();

                    string phrase = ParamsToString(args);
                    DiscordEmbedBuilder deb;

                    if (!Excludes.IsExcluded(phrase))
                    {
                        deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: @"Unable to Remove Exclude",
                            description: ChatObjects.GetErrMessage(@"I was unable to remove that phrase from exclusion. It's not excluded already..."),
                            color: ChatObjects.ErrColor,
                            thumbnail: ChatObjects.URL_FILTER_SUB
                        ));
                    }
                    else
                    {
                        Excludes.RemovePhrase(phrase);
                        Excludes.Save();

                        deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: @"Removed Exclude",
                            description: ChatObjects.GetSuccessMessage(@"I removed that phrase from exclusion!"),
                            color: ChatObjects.SuccessColor,
                            thumbnail: ChatObjects.URL_FILTER_SUB
                        ));
                    }

                    deb.AddField(@"Phrase", phrase);

                    await ctx.Channel.SendMessageAsync(embed: deb.Build());
                }
            }
        }

        [Command("filterexcludes")]
        public async Task ListFilterExcludes(CommandContext ctx)
        {
            if (!ctx.Member.GetHighestRole().IsModOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                       (
                           requiredRole: Role.Moderator,
                           command: ctx.Command.Name,
                           channel: ctx.Channel,
                           caller: ctx.Member
                       );
            }
            else
            { 
                var stringBuilder = new StringBuilder();

                foreach (string word in Excludes.GetPhrases())
                    stringBuilder.AppendLine($"`{word}`");

                await ctx.Channel.SendMessageAsync(
                    embed:

                    ChatObjects.FormatEmbedResponse
                    (
                        title: "List of Excludes",
                        description: stringBuilder.ToString(),
                        color: ChatObjects.NeutralColor,
                        thumbnail: ChatObjects.URL_FILTER_GENERIC
                    ));
            }
        }

        #endregion Word Filter

        private static string ParamsToString(string[] paramsStr)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendJoin(' ', paramsStr);

            return stringBuilder.ToString();
        }

    }
}
