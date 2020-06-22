﻿// WarnCommands.cs
// Contains commands for searching user warns. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;

namespace FluffyEars.Commands
{
    class WarnCommands : BaseCommandModule
    {

        public WarnCommands() { }

        [Command("setactionchan")]
        public async Task SetWarnChannel(CommandContext ctx, DiscordChannel chan)
        {
            // Check if the user can use these sorts of commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                DiscordEmbed embedResponse;

                await ctx.TriggerTypingAsync();

                if (chan.GuildId.Equals(ctx.Guild.Id))
                {   // Only continue if this is the same guild.
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Action Logs channel set",
                            description: ChatObjects.GetSuccessMessage($"Action Logs channel set to {chan.Mention}!"),
                            color: ChatObjects.SuccessColor
                        );

                    BotSettings.ActionChannelId = chan.Id;
                    BotSettings.Save();
                }
                else
                {   // Not the same guild
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Unable to set Action Logs channel",
                            description: ChatObjects.GetErrMessage($"ActionLog channel not set. That's an invalid channel..."),
                            color: ChatObjects.ErrColor
                        );
                }

                await ctx.Channel.SendMessageAsync(embed: embedResponse);
            }
        }

        private const string WARN_SEEK_COMMAND = "mentions";
        [Command(WARN_SEEK_COMMAND)]
        public async Task SeekWarns(CommandContext ctx, params DiscordMember[] members)
        {
            // Check if the user can use these sorts of commands.
            if (!ctx.Member.GetHighestRole().IsCSOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.CS,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                DiscordChannel actionLogChannel = await Bot.BotClient.GetChannelAsync(BotSettings.ActionChannelId);

                Dictionary<DiscordMember, List<DiscordMessage>> warnDict =
                    await QueryMemberMentions(members.Distinct().ToList(), actionLogChannel, BotSettings.WarnThreshold, ctx.Message);

                // Let's start paginating.
                var pages = new Page[warnDict.Keys.Count];
                int page = 0;

                foreach (var member in warnDict.Keys)
                {   // Want to generate a page for each member.

                    // We want a boolean to check first because if there's no key, we'll get an exception trying to get the count.
                    bool warnsFound = warnDict.ContainsKey(member) && warnDict[member].Count > 0;

                    var deb = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: "Discord Mentions",
                            description: ChatObjects.GetNeutralMessage(warnsFound ?
                                                                       $"{ctx.Member.Mention}, I found {warnDict[member].Count} mentions for {member.Mention} in {actionLogChannel.Mention} in the last {BotSettings.WarnThreshold} months. {(warnDict[member].Count > 25 ? "There are over 25. I will only show the most recent." : String.Empty)}" :
                                                                       $"{ctx.Member.Mention}, I did not find any warnings for {member.Mention}. Good for them..."),
                            color: warnsFound ? DiscordColor.Green : DiscordColor.Red
                        ));

                    if (warnsFound)
                    {   // Only continue here if there are actually warns, otherwise just slap a footer on.
                        foreach (var message in warnDict[member])
                        {   // Generate a field for each detected message.

                            if (deb.Fields.Count < 25)
                            {   // Only continue if we have less than 25 fields.
                                deb.AddField($"Action on {message.Timestamp.ToString(ChatObjects.DateFormat)}",
                                    $"{ChatObjects.PreviewString(message.Content, 1000)}\n{Formatter.MaskedUrl(@"Link", new Uri(ChatObjects.GetMessageUrl(message)))}");
                            }
                            else
                            {   // Stop the loop if we have 25 fields.
                                break; // NON-SESE ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! !
                            } // end else
                        } // end foreach
                    } // end if

                    deb.WithFooter($"Page {page + 1}/{warnDict.Keys.Count}");

                    pages[page++] = new Page(embed: deb);
                } // end foreach

                // Delete the message so it's kind of out of the way and doesn't get logged again in the future.
                await ctx.Message.DeleteAsync();

                var interactivity = Bot.BotClient.GetInteractivity();

                await interactivity.SendPaginatedMessageAsync
                    (
                        c: ctx.Channel,
                        u: ctx.User,
                        pages: pages,
                        emojis: ChatObjects.DefaultPaginationEmojis
                    );
            }
        }

        [Command("setactionthreshold")]
        public async Task SetWarnThreshold(CommandContext ctx, int months)
        {
            // Check if the user can use these sorts of commands.
            if (!ctx.Member.GetHighestRole().IsBotManagerOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.BotManager,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                DiscordEmbed embedResponse;

                await ctx.TriggerTypingAsync();

                if (months > 0)
                {   // Only continue if there's a valid value.
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Warn threshold set",
                            description: ChatObjects.GetSuccessMessage($"Warn threshold set to {months} months!"),
                            color: ChatObjects.SuccessColor
                        );

                    BotSettings.WarnThreshold = months;
                    BotSettings.Save();
                }
                else
                {
                    embedResponse = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Unable to set warn threshold",
                            description: ChatObjects.GetErrMessage($"Warn threshold not set. That's zero months..."),
                            color: ChatObjects.ErrColor
                        );
                }

                await ctx.Channel.SendMessageAsync(embed: embedResponse);
            }
        }

        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Channel.Id == BotSettings.ActionChannelId &&
                !(e.Message.GetMentionPrefixLength(Bot.BotClient.CurrentUser) != -1 && e.Message.Content.Contains(WARN_SEEK_COMMAND)))
            {   // Only continue if this is the action channel, and it doesn't look like a command.

                // ----
                // Get the DiscordMember of each user.

                var mentionedColonists = new List<DiscordMember>();

                foreach(var user in e.MentionedUsers)
                {
                    var member = await e.Guild.GetMemberAsync(user.Id);
                    
                    if(member.GetHighestRole().Equals(Role.Colonist) && !member.IsBot)
                    {   // Only add this person if they're a colonist and not a bot.

                        mentionedColonists.Add(member);
                    } // end if
                } // end foreach

                // ----
                // Great, now we have all our mentioned colonists. Let's just make sure there's no duplicates.
                mentionedColonists = mentionedColonists.Distinct().ToList();

                if(mentionedColonists.Count > 0)
                {   // Only continue if there's actually mentioned colonists.

                    var actionChannel = e.Channel;

                    // Create a dictionary based on a DiscordMember and all of the messages mentioning him or her.
                    Dictionary<DiscordMember, List<DiscordMessage>> warnDict = 
                        await QueryMemberMentions(mentionedColonists, actionChannel, BotSettings.WarnThreshold, e.Message);

                    // ----
                    // So at this point, we know there's at least one person who has been warned (0,inf) times.
                    // Now we want to act on each warn, constructing them into a DiscordEmbed.

                    // For each member...
                    foreach (DiscordMember key in warnDict.Keys)
                    {
                        List<DiscordMessage> messages = warnDict[key];

                        if (messages.Count > 0)
                        {   // We found warns for this user! How unfortunate...

                            var embedBase = new DiscordEmbedBuilder();
                            var stringBuilder = new StringBuilder();

    
                            stringBuilder.Append($"__**{key.Mention} has {messages.Count} mentions in {e.Channel.Mention}:**__\n");

                            int count = 0;
                            stringBuilder.AppendJoin(' ',
                                messages.Select(a =>    // Generate a bunch of masked urls
                                    Formatter.MaskedUrl($"#{++count}", new Uri(ChatObjects.GetMessageUrl(a)))));

                            string finalStr = ChatObjects.PreviewString(stringBuilder.ToString(), 2048);

                            embedBase.WithDescription(finalStr);
                            embedBase.WithTitle(@"Previous mentions found");
                            embedBase.WithColor(DiscordColor.Red);

                            await e.Channel.SendMessageAsync(embed: embedBase);
                        }
                    }                    
                } // end if
            } // end if
        } // end method

        /// <summary>Query for all mentions of specific members.</summary>
        /// <param name="members">The members to query for.</param>
        /// <param name="actionChannel">#action-logs</param>
        /// <param name="warnThreshold">How long ago to query in months.</param>
        /// <param name="originalMessage">The original message.</param>
        /// <returns></returns>
        private static async Task<Dictionary<DiscordMember, List<DiscordMessage>>> 
            QueryMemberMentions(List<DiscordMember> members,  
                                DiscordChannel actionChannel, 
                                int warnThreshold,
                                DiscordMessage originalMessage)
        {
            const int MESSAGE_COUNT = 2000;
            // Remember to use DTO in the current timezone and not in UTC! API is running on OUR time.
            DateTime startTime = DateTime.Now.AddMonths(warnThreshold * -1);

            // We want to set the initial messages. This will get the most recent 100 messages, excluding the message that triggered this.
            var messages = await actionChannel.GetMessagesBeforeAsync(originalMessage.Id, MESSAGE_COUNT);

            // We want a "stop" value, so to speak. If this is true, it means we've gone before startTime.
            bool exceededStartTime = false;

            // Every instance where this user has been mentioned.
            var warnInstances = new Dictionary<DiscordMember, List<DiscordMessage>>();

            do
            {
                if(messages.Count > 0)
                {
                    foreach(var message in messages)
                    {   // For each message, we want to check its mentioned users.

                        if(startTime.Millisecond <= message.CreationTimestamp.Ticks)
                        {   // We only want to continue if this is after our startValue.

                            foreach (var member in members)
                            {
                                if (MentionedUsersContains(message, member))
                                {   // Only continue if there are actually mentioned users, and if the mentioned users has the member we want.

                                    if (!warnInstances.ContainsKey(member))
                                    {
                                        warnInstances.Add(member, new List<DiscordMessage>());
                                    }

                                    warnInstances[member].Add(message);

                                } // end if
                            } // end foreach
                        } // end if
                        else
                        {   // We've gone way too far back, so we need to stop this.
                            exceededStartTime = true;
                            break;  // Break out of the foreach. NON-SESE ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! !
                        } // end else
                    } // end foreach

                    if (!exceededStartTime && messages.Count == MESSAGE_COUNT)
                    {   // Only repopulate if we're still within the time range AND if there are still messages to grab. My logic behind checking the
                        // message count is that there should be 2,000 or more messages if there's still messages to be found, unless the message
                        // count is exactly a multiple of 2,000.
                        messages = await actionChannel.GetMessagesBeforeAsync(messages.Last().Id, MESSAGE_COUNT);

                        // Give the bot some time to process.
                        await Task.Delay(500);
                    }
                    else
                    {   // Stop the loop.
                        exceededStartTime = false;
                        messages = default;
                    }// end else
                } // end if
            } while (!exceededStartTime && !(messages is null) && messages.Count > 0);
            // ^ Only continue if we haven't gone back far enough in history AND if we still have messages.

            return warnInstances;
        }

        /// <summary>Checks if a message contains the specified user.</summary>
        private static bool MentionedUsersContains(DiscordMessage message, DiscordMember member)
        {
            bool returnVal = false;

            if (message.MentionedUsers.Count > 0)
            {
                foreach(var user in message.MentionedUsers)
                {
                    if (returnVal)
                    {
                        break;// NON-SESE ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! 
                    }

                    try
                    {   // For some reason, this can throw an exception. I can't figure out why, and honestly, I don't quite care to figure it out.
                        if (user.Id == member.Id)
                        {
                            returnVal = true;
                            break; // NON-SESE ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! 
                        }
                    }
                    catch { }
                }
            }

            return returnVal;
        }
    }
}
