// ReminderCommands.cs
// Contains a module for setting, removing, and listing reminders.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using FluffyEars.Reminders;
using System.Text.RegularExpressions;

namespace FluffyEars.Commands
{
    class ReminderCommands : BaseModule
    {
        static Regex DateRegex
            = new Regex(@"(\d+)\s?(months?|days?|weeks?|wks?|hours?|hrs?|minutes?|mins?)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        [Command("+reminder")]
        public async Task AddReminder(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsCHOrHigher())
            {
                
                string args = ctx.RawArgumentString;
                int firstQuote = args.IndexOf('[');
                int secondQuote = args.LastIndexOf(']');

                // Let's try to get the date, first of all.
                string dateSubstring;

                // If there's no quote, let's just see what happens if we put the whole string in.
                if (firstQuote == -1)
                    dateSubstring = args.TrimStart();
                else dateSubstring = args.TrimStart(' ').Substring(0, firstQuote);

                MatchCollection matches = DateRegex.Matches(dateSubstring);

                DateTimeOffset dto = ctx.Message.CreationTimestamp.UtcDateTime;

                foreach(Match match in matches)
                {
                    if(match.Groups.Count == 3)
                    {
                        // Check if it's an integer just in case...
                        if (Int32.TryParse(match.Groups[1].Value, out int measure))
                        {
                            InterpretTime(
                                measure: measure,
                                unit: match.Groups[2].Value,
                                dto: ref dto);
                        }
                    }
                }

                // Cancels: ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! !

                if (dto.UtcTicks == ctx.Message.CreationTimestamp.UtcTicks)
                {   // No time has been added.
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"Invalid time string..."));
                    return;
                }

                DateTimeOffset yearFromNow = new DateTimeOffset(ctx.Message.CreationTimestamp.UtcDateTime).AddYears(1);
                if(dto.UtcTicks > yearFromNow.UtcTicks)
                {   // More than a year away.
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"That's more than one year away. Please reduce your time..."));
                    return;
                }

                // Great, so now we have our time string. Now we need to try and figure out what our message string is.
                string msgString = @"no message provided";
                
                // Just checking to make sure everything is in bounds.
                if(firstQuote != -1 && firstQuote + 1 < args.Length && secondQuote - 1 > firstQuote)
                {
                    if (secondQuote == -1)
                        msgString = args.Substring(firstQuote + 1);
                    else msgString = args.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                }

                // Now we have our message string. Let's see if there are any mentions.
                ulong[] mentionIds = ctx.Message.MentionedUsers.Select(a => a.Id).ToArray();


                StringBuilder sb = new StringBuilder();
                // DEB!
                DiscordEmbedBuilder deb = new DiscordEmbedBuilder();
                
                Reminder reminder = new Reminder
                {
                    Text = msgString,
                    Time = dto.ToUnixTimeMilliseconds(),
                    User = ctx.Member.Id,
                    Channel = ctx.Channel.Id,
                    UsersToNotify = mentionIds
                };

                foreach (ulong mentionId in mentionIds)
                {
                    if (mentionId != 669347771312111619)
                        sb.Append(String.Format("<@{0}> ", mentionId));
                }

                deb.WithTitle(@"Notification");
                deb.AddField(@"User", ctx.Member.Mention);
                deb.AddField(@"Time", dto.ToString());
                deb.AddField(@"Message", msgString);

                if (sb.Length > 0)
                    deb.AddField(@"Users to notify:", sb.ToString().TrimEnd());

                deb.AddField(@"Notification Identifier", reminder.GetIdentifier());
                deb.WithThumbnailUrl(ChatObjects.URL_REMINDER_GENERIC);

                await ctx.Channel.SendMessageAsync(String.Empty, false, deb);
                await SelfAudit.LogSomething(ctx.User, @"+reminder", String.Join('\n', msgString, dto.ToString()));

                ReminderSystem.AddReminder(reminder);
                ReminderSystem.Save();
            }
        }

        [Command("-reminder")]
        public async Task RemoveReminder(CommandContext ctx, string reminderId)
        {
            if(ctx.Member.GetRole().IsCHOrHigher())
            {
                await ctx.TriggerTypingAsync();

                // Cancels:
                if(!ReminderSystem.IsReminder(reminderId))
                {
                    await ctx.Channel.SendMessageAsync(
                        ChatObjects.GetErrMessage(@"That is not a valid Reminder ID..."));
                    return;
                }

                Reminder reminderToRemove = ReminderSystem.GetReminderFromId(reminderId);

                // DEB!
                DiscordEmbedBuilder deb = new DiscordEmbedBuilder();

                DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(reminderToRemove.Time); // The reminder's DTO.
                TimeSpan remainingTime = dto.Subtract(DateTimeOffset.UtcNow); // The remaining time left for the reminder.
                string originalAuthorMention = String.Format("<@{0}>", reminderToRemove.User);

                deb.WithTitle(@"Notification Removed");
                deb.AddField(@"User", originalAuthorMention);
                deb.AddField(@"Time", dto.ToString());
                deb.AddField(@"Remaining time", 
                    String.Format("{0}day {1}hr {2}min {3}sec", remainingTime.Days, remainingTime.Hours, remainingTime.Minutes, remainingTime.Seconds));
                deb.AddField(@"Message", reminderToRemove.Text);
                deb.AddField(@"Notification Identifier", reminderId);

                deb.WithColor(DiscordColor.LightGray);
                deb.WithThumbnailUrl(ChatObjects.URL_REMINDER_DELETED);
            
                await ctx.Channel.SendMessageAsync(originalAuthorMention, false, deb);
                await SelfAudit.LogSomething(ctx.User, @"-reminder", String.Join('\n', originalAuthorMention, dto.ToString(), reminderId, reminderToRemove.Text));

                ReminderSystem.RemoveReminder(reminderToRemove);
                ReminderSystem.Save();
            }
        }

        [Command("reminderlist")]
        public async Task ListReminders(CommandContext ctx)
        {
            if(ctx.Member.GetRole().IsCHOrHigher())
            {
                // Check if there are any notifications. If there are none, let the user know.
                if (ReminderSystem.HasNotification())
                {
                    int debLength;
                    int page = 1;

                    // DEB!
                    DiscordEmbedBuilder deb = new DiscordEmbedBuilder();
                    deb.WithTitle(@"Reminder List Page " + page);
                    deb.WithThumbnailUrl(@"https://i.imgur.com/lOqo2k8.png");
                    debLength = deb.ThumbnailUrl.Length;

                    // Get a list of reminders, but ordered descending by their remind date.
                    Reminder[] reminderList = ReminderSystem.GetReminders().OrderByDescending(a => a.Time).ToArray();
                    foreach (Reminder reminder in reminderList)
                    {
                        int reminderLength;
                        StringBuilder sb = new StringBuilder();

                        Array.ForEach(reminder.UsersToNotify,            // For every user (a), append them to sb in mention format <@id>.
                            a => sb.Append(String.Format("<@{0}> ", a)));

                        DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(reminder.Time);

                        string str1 = dto.ToString("ddMMMyyyy HH:mm");
                        string str2 = String.Format("<@{0}>: {1}\nMentions: {2}\nId: {3}",
                            /*{0}*/ reminder.User, /*{1}*/ reminder.Text, /*{2}*/ sb.ToString(), /*{3}*/ reminder.GetIdentifier());

                        reminderLength = sb.Length + str1.Length + str2.Length + deb.Title.Length;


                        // So, if the resulting length is > 1600, we wanna send it and clear the embed.
                        if (reminderLength + debLength > 1600)
                        {
                            await ctx.Channel.SendMessageAsync(embed: deb);
                            deb = new DiscordEmbedBuilder();
                            deb.WithTitle(@"Reminder List Page " + ++page);
                            deb.WithThumbnailUrl(ChatObjects.URL_REMINDER_GENERIC);
                            debLength = deb.ThumbnailUrl.Length;
                        }

                        deb.AddField(str1, str2);
                        debLength += reminderLength;

                        // This checks for cases where we just finished with an embed, but we have one more reminder left. So if we do, let's send 
                        // it.
                        if(reminder.Equals(reminderList.Last()))
                            await ctx.Channel.SendMessageAsync(embed: deb);
                    }

                } else await ctx.Channel.SendMessageAsync("There are no notifications.");
            }
        }
        private static void InterpretTime(int measure, string unit, ref DateTimeOffset dto)
        {
            // Only continue if these two have a valid value.
            if (measure > 0 && unit.Length > 0)
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

        protected override void Setup(DiscordClient client) { }
    }
}
