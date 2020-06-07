﻿// ReminderCommands.cs
// Contains a module for setting, removing, and listing reminders.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyEars.Reminders;
using System.Text.RegularExpressions;
using System.Collections;

namespace FluffyEars.Commands
{
    class ReminderCommands : BaseCommandModule
    {
        static Regex DateRegex
            = new Regex(@"(\d+)\s?(months?|days?|weeks?|wks?|hours?|hrs?|minutes?|mins?)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        public ReminderCommands() { }

        [Command("+reminder")]
        public async Task AddReminder(CommandContext ctx, params string[] paramsList)
        {
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
                await ctx.TriggerTypingAsync();

                string args = ctx.RawArgumentString;

                // Firstly get all the matches.
                MatchCollection regexMatches = DateRegex.Matches(args);
                BitArray regexCoverage = new BitArray(args.Length);
                var dto = ctx.Message.CreationTimestamp;
                List<ulong> mentions = new List<ulong>();

                // String processing - find the message and get the reminder end date. 
                // To find what's not a date, we simply look for the first character that isn't in the boundaries of a Regex match. 
                //
                // Structure of a possible string:
                //
                //             Date String | Message String
                //  DATE, DATE, DATE, DATE | message
                //
                // Beyond the Date String we want to stop processing time information as people may reference time in the message string, so we don't
                // erronously want that data added to the date string.


                // Populate the regexCoverage...
                foreach (Match match in regexMatches)
                {
                    for(int i = match.Index; i < match.Index + match.Length; i++)
                    {
                        regexCoverage[i] = true;
                    }
                }

                // So I want to explain what I'm about to do here. Every value in regexCoverage[] indicates if that's part of the initial time
                // string. We want to use it to determine if something is a message or a time value, so if at any point, we run into something that
                // isn't a time string, we want to set every instance thereafter as false so we know it's part of a message.
                if (regexMatches.Count > 0)
                {
                    bool value = regexCoverage[0];
                    for (int k = 1; k < regexCoverage.Count; k++)
                    {
                        if (!IsWhitespace(args[k]))
                        {
                            if (!regexCoverage[k] && value)
                            {
                                value = false;
                            }

                            if (!value)
                            {
                                regexCoverage[k] = value;
                            }
                        }
                    }
                }
                // We need to figure out where the date string ends.
                string messageString = String.Empty;

                int dateEndIndex = 0;
                bool messageFound = false;
                while(dateEndIndex < regexCoverage.Length && !messageFound) 
                {
                    char stringChar = args[dateEndIndex];
                    bool inRegexBoundaries = regexCoverage[dateEndIndex];

                    // This checks to see if the character is non-white-space and outside of any RegEx boundaries.
                    messageFound = !IsWhitespace(stringChar) && !inRegexBoundaries;

                    // If not found, continue; otherwise, keep incrementing.
                    if(!messageFound)
                    {
                        dateEndIndex++;
                    }
                }

                // If we aren't going out of bounds, let's set the string to this.
                if(dateEndIndex < regexCoverage.Length)
                {
                    messageString = args.Substring(dateEndIndex);
                }

                // Get date information
                foreach(Match match in regexMatches)
                {
                    // Only try to exclude Message String date information if a message string was found.
                    if(!messageFound || (regexCoverage[match.Index] && regexCoverage[match.Index + match.Length - 1]))
                    {
                        InterpretTime(match.Groups[1].Value, match.Groups[2].Value, ref dto);
                    }
                }

                // Get mentions
                foreach(DiscordUser user in ctx.Message.MentionedUsers)
                {
                    ulong id = user.Id;

                    if(!user.IsBot && !mentions.Contains(id))
                    {
                        mentions.Add(id);
                    }
                }

                // At this point, now we have the DateTimeOffset describing when this reminder needs to be set off, and we have a message string if
                // any. So now we just need to make sure it's within reasonable boundaries, set the reminder, and notify the user.

                DateTimeOffset yearFromNow = new DateTimeOffset(ctx.Message.CreationTimestamp.UtcDateTime).AddYears(1);
                DiscordEmbed embed;
                Reminder reminder;

                if (dto.UtcTicks == ctx.Message.CreationTimestamp.UtcTicks)
                {   // No time was added.
                    embed = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Unable to Add Reminder",
                            description: ChatObjects.GetErrMessage(@"I was unable able to add the mask you gave me. You didn't supply me a valid time..."),
                            color: ChatObjects.ErrColor,
                            thumbnail: ChatObjects.URL_REMINDER_GENERIC
                        );
                }
                else if (dto.UtcTicks > yearFromNow.UtcTicks)
                {   // More than a year away.
                    embed = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Unable Add Reminder",
                            description: ChatObjects.GetErrMessage(@"I was unable able to add the mask you gave me. That's more than a year away..."),
                            color: ChatObjects.ErrColor,
                            thumbnail: ChatObjects.URL_REMINDER_GENERIC
                        );
                }
                else
                {
                    embed = ChatObjects.FormatEmbedResponse
                        (
                            title: @"Add Reminder",
                            description: ChatObjects.GetSuccessMessage(@"I added the reminder you gave me!"),
                            color: ChatObjects.SuccessColor,
                            thumbnail: ChatObjects.URL_REMINDER_GENERIC
                        );

                    reminder = new Reminder
                    {
                        Text = messageString,
                        Time = dto.ToUnixTimeMilliseconds(),
                        User = ctx.Member.Id,
                        Channel = ctx.Channel.Id,
                        UsersToNotify = mentions.ToArray()
                    };

                    ReminderSystem.AddReminder(reminder);
                    ReminderSystem.Save();

                    var deb = new DiscordEmbedBuilder(embed);

                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendJoin(' ', mentions.ToArray().Select(a => ChatObjects.GetMention(a)));

                    deb.AddField(@"User", ctx.Member.Mention, true);
                    deb.AddField(@"Time", dto.ToString(), true);
                    deb.AddField(@"Notification Identifier", reminder.GetIdentifier(), false);

                    if (stringBuilder.Length > 0)
                        deb.AddField(@"Users to notify:", stringBuilder.ToString(), false);

                    deb.AddField(@"Message",messageString, false);

                    embed = deb.Build();
                } // end else

                await ctx.Channel.SendMessageAsync(embed: embed);
            } // end else
        } // end method

        [Command("-reminder")]
        public async Task RemoveReminder(CommandContext ctx, string reminderId)
        {
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
                await ctx.TriggerTypingAsync();

                // Cancels:
                if (!ReminderSystem.IsReminder(reminderId))
                {
                    await ctx.Channel.SendMessageAsync(
                        embed:
                        
                        ChatObjects.FormatEmbedResponse
                        (
                            title: @"Unable to Remove Reminder",
                            description: ChatObjects.GetErrMessage(@"I was unable to remove the reminder you gave me. That's an invalid ID..."),
                            color: ChatObjects.ErrColor,
                            thumbnail: ChatObjects.URL_REMINDER_GENERIC
                        ));
                }
                else
                {
                    Reminder reminderToRemove = ReminderSystem.GetReminderFromId(reminderId);

                    var discordEmbedBuilder = new DiscordEmbedBuilder(ChatObjects.FormatEmbedResponse
                        (
                            title: @"Remove Reminder",
                            description: ChatObjects.GetSuccessMessage(@"I able to remove the reminder you gave me!"),
                            color: ChatObjects.SuccessColor,
                            thumbnail: ChatObjects.URL_REMINDER_GENERIC
                        ));

                    DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(reminderToRemove.Time); // The reminder's DTO.
                    TimeSpan remainingTime = dto.Subtract(DateTimeOffset.UtcNow); // The remaining time left for the reminder.
                    string originalAuthorMention = ChatObjects.GetMention(reminderToRemove.User);

                    discordEmbedBuilder.AddField(@"User", originalAuthorMention, true);
                    discordEmbedBuilder.AddField(@"Time", dto.ToString(), true);
                    discordEmbedBuilder.AddField(@"Notification Identifier", reminderId, false);
                    discordEmbedBuilder.AddField(@"Remaining time",
                        String.Format("{0}day {1}hr {2}min {3}sec", remainingTime.Days, remainingTime.Hours, remainingTime.Minutes, remainingTime.Seconds), false);
                    discordEmbedBuilder.AddField(@"Message", reminderToRemove.Text, false);

                    ReminderSystem.RemoveReminder(reminderToRemove);
                    ReminderSystem.Save();

                    await ctx.Channel.SendMessageAsync(originalAuthorMention, embed: discordEmbedBuilder);
                } // end else
            } // end else
        } // end method

        [Command("reminderlist")]
        public async Task ListReminders(CommandContext ctx)
        {
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
                // Check if there are any notifications. If there are none, let the user know.
                if (ReminderSystem.HasNotificationsPending())
                {
                    var interactivity = ctx.Client.GetInteractivity();
                    List<Page> pages = new List<Page>();

                    Reminder[] reminderList = ReminderSystem.GetReminders().OrderByDescending(a => a.Time).ToArray();

                    var deb = new DiscordEmbedBuilder();

                    int count = 0;

                    const int REMINDERS_PER_PAGE = 5;
                    for (int i = 0; i < reminderList.Length; i++)
                    {
                        Reminder reminder = reminderList[i];

                        if (i % REMINDERS_PER_PAGE == 0 || i == reminderList.Length - 1)
                        {
                            deb.WithDescription(ChatObjects.GetNeutralMessage($"Hello {ctx.Member.Mention}, please note you are the only one who can react to this message.\n\n**Showing {count} reminders out of a total of {reminderList.Length}.**"));

                            if (i != 0)
                            {
                                pages.Add(new Page(embed: deb));
                                count = 0;
                            }

                            deb = new DiscordEmbedBuilder();

                            deb.WithTitle($"Reminders Page {Math.Ceiling((i + 1.0f) / (float)REMINDERS_PER_PAGE)}/{(reminderList.Length + reminderList.Length % REMINDERS_PER_PAGE) / REMINDERS_PER_PAGE}");
                            deb.WithColor(ChatObjects.NeutralColor);
                            deb.WithThumbnail(ChatObjects.URL_REMINDER_GENERIC);
                        }

                        var stringBuilder = new StringBuilder();
                        // For every user (a), append them to sb in mention format <@id>.
                        Array.ForEach(reminder.UsersToNotify, a => stringBuilder.Append($"{ChatObjects.GetMention(a)} "));

                        var dto = DateTimeOffset.FromUnixTimeMilliseconds(reminder.Time);

                        string name = dto.ToString("ddMMMyyyy HH:mm");
                        string value = $"{ChatObjects.GetMention(reminder.User)}: {reminder.Text}\nMentions: {stringBuilder.ToString().TrimEnd()}\nId: {reminder.GetIdentifier()}";

                        deb.AddField(name, value);
                        count++;
                    }

                    var emojis = new PaginationEmojis
                    {
                        Left = DiscordEmoji.FromName(Bot.BotClient, @":arrow_backward:"),
                        Right = DiscordEmoji.FromName(Bot.BotClient, @":arrow_forward:"),
                        Stop = DiscordEmoji.FromName(Bot.BotClient, @":stop_button:"),
                        SkipLeft = null,
                        SkipRight = null
                    };

                    await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages, emojis: emojis);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(
                        embed:
                    
                        ChatObjects.FormatEmbedResponse
                            (
                                title: @"Reminders",
                                description: ChatObjects.GetNeutralMessage(@"There are no reminders."),
                                color: ChatObjects.NeutralColor,
                                thumbnail: ChatObjects.URL_REMINDER_GENERIC
                            ));
                } // end else
            } // end if
        } // end method

        private static void InterpretTime(string measureString, string unit, ref DateTimeOffset dto)
        {
            // Only continue if these two have a valid value.
            if (int.TryParse(measureString, out int measure) && measure > 0 && unit.Length > 0)
            {
                switch (unit.ToLower())
                {
                    case "month":
                    case "months":
                        dto = dto.AddMonths(measure);
                        break;
                    case "day":
                    case "days":
                        dto = dto.AddDays(measure);
                        break;
                    case "week":
                    case "weeks":
                    case "wk":
                    case "wks":
                        dto = dto.AddDays(measure * 7);
                        break;
                    case "hour":
                    case "hours":
                    case "hr":
                    case "hrs":
                        dto = dto.AddHours(measure);
                        break;
                    case "minute":
                    case "minutes":
                    case "min":
                    case "mins":
                        dto = dto.AddMinutes(measure);
                        break;
                    default:
                        break;
                }
            }
        }

        private static bool IsWhitespace(char c)
        {
            return c.Equals(' ') ||
                   c.Equals('\r') ||
                   c.Equals('\n') ||
                   c.Equals('\t');
        }
    }
}
