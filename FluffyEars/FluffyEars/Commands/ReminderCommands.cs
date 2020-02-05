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

namespace FluffyEars.Commands
{
    class ReminderCommands : BaseModule
    {
        [Command("+reminder"), 
            Description("[CH+] Add a reminder.\nUsage: +reminder \\`time [month(s), week(s) day(s), hour(s), minute(s)\\` \\`reminder message\\` @mention1 @mention2 ... @mention_n\nhttps://i.imgur.com/H1fVPta.png")]
        public async Task AddReminder(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsCHOrHigher())
            {
                // DEB!
                DiscordEmbedBuilder deb = new DiscordEmbedBuilder();

                // If there are four `s, that means we have a valid argument.
                // Basically, it's @bot addreminder `time` `message`
                if (ctx.RawArgumentString.Count(a => a == '`') == 4)
                {
                    string[] spaghetti = ctx.RawArgumentString.Split('`');
                    string timeString = spaghetti[1];
                    string messageString = spaghetti[3];

                    if (messageString.Length > 160)
                    {
                        ctx.Channel.SendMessageAsync("Too many characters.");
                        return;
                    }

                    // Default message.
                    if (messageString.Trim() == String.Empty)
                        messageString = "Untitled notification";

                    // Check how many spaces there are. If there are none, that means there really isn't anything to take in.
                    if (timeString.Count(a => a == ' ') < 1)
                        await ctx.Channel.SendMessageAsync(@"Invalid time");
                    else
                    {
                        DateTimeOffset dto = InterpretTimeString(timeString);

                        if (dto.CompareTo(DateTimeOffset.UtcNow) != -1 &&
                            DateTimeOffset.UtcNow.AddYears(1).UtcTicks >= dto.UtcTicks)
                        {
                            StringBuilder sb = new StringBuilder();

                            // For every userId that doesn't equal the bot's userId, add them to the list.
                            List<ulong> usersToNotify = new List<ulong>();
                            int i = 0;
                            foreach (ulong userId in ctx.Message.MentionedUsers.Select(a => a.Id))
                                if (userId != Bot.BotClient.CurrentUser.Id)
                                    usersToNotify.Add(userId);

                            // Generate a new reminder!
                            Reminder reminder = new Reminder
                            {
                                Text = messageString,
                                Time = dto.ToUnixTimeMilliseconds(),
                                User = ctx.Member.Id,
                                Channel = ctx.Channel.Id,
                                UsersToNotify = usersToNotify.ToArray()
                            };

                            foreach (string mention in ctx.Message.MentionedUsers.Select(a => a.Mention))
                                if (mention != @"<@!669347771312111619>")
                                    sb.Append(mention + ' ');

                            deb.WithTitle(@"Notification");
                            deb.AddField(@"User", ctx.Member.Mention);
                            deb.AddField(@"Time", dto.ToString());
                            deb.AddField(@"Message", messageString);

                            if (sb.Length > 0)
                                deb.AddField(@"Users to notify:", sb.ToString().TrimEnd());

                            deb.AddField(@"Notification Identifier", reminder.GetIdentifier());

                            ReminderSystem.AddReminder(reminder);

                            await ctx.Channel.SendMessageAsync(String.Empty, false, deb);
                            ReminderSystem.Save();
                        }
                        else await ctx.Channel.SendMessageAsync("Invalid arguments.");
                    }
                }
                else
                    await ctx.Channel.SendMessageAsync(@"Unrecognized arguments.");

            }
        }

        [Command("-reminder"),
            Description("[CH+] Removes a reminder.\nUsage: -reminder reminder_id")]
        public async Task RemoveReminder(CommandContext ctx, string reminderId)
        {
            // Only enter scope if (a) user can use reminder commands && this is an existing reminder.
            if(ctx.Member.GetRole().IsCHOrHigher() && ReminderSystem.IsReminder(reminderId))
            {
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

                ReminderSystem.RemoveReminder(reminderToRemove);

                await ctx.Channel.SendMessageAsync(originalAuthorMention, false, deb);
                ReminderSystem.Save();
            }
        }

        [Command("listreminders"),
            Description("[CH+] Lists all pending notifications.")]
        public async Task ListReminders(CommandContext ctx)
        {
            if(ctx.Member.GetRole().IsCHOrHigher())
            {
                // Check if there are any notifications. If there are none, let the user know.
                if (ReminderSystem.HasNotification())
                {
                    int debLength = 0;
                    int page = 1;

                    // DEB!
                    DiscordEmbedBuilder deb = new DiscordEmbedBuilder();
                    deb.WithTitle(@"Reminder List Page " + page);

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
                            debLength = 0;
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

        private static DateTimeOffset InterpretTimeString(string str)
        {
            DateTimeOffset dto = DateTimeOffset.UtcNow;
            string[] spaghetti = str.Split(' '); // An array of either a potential number or a potential unit of measure.

            int lastNumber = 0;
            for (int i = 0; i < spaghetti.Length; i++)
            {
                int curNumber;
                string meatball = spaghetti[i];

                // If this isn't a number, it's probably some kind of unit of measurement.
                // Regardless of if it's a number or not, it tries to update the last number. If it is not a number, enter the switch/case
                // and figure out what unit of measurement it is. If it is a number, enter the else scope and update the last number.
                if (!int.TryParse(meatball, out curNumber))
                {
                    if (lastNumber != 0)
                    {
                        // Because the last number is always updated when a number is found, this means a number has to precede the 
                        // unit of measurement. Theoretically, something like 1 month minute could add 1 month and one minute. That's on the user, 
                        // though.
                        switch (meatball.ToLower())
                        {
                            case "month":
                            case "months":
                            case "mo":
                            case "mos": // Months.
                                dto = dto.AddMonths(lastNumber);
                                break;
                            case "week":
                            case "weeks":
                            case "wk":
                            case "wks":
                            case "w":
                                dto = dto.AddDays(lastNumber * 7);
                                break;
                            case "day":
                            case "days":
                            case "d":      // Days.
                                dto = dto.AddDays(lastNumber);
                                break;
                            case "hour":
                            case "hours":
                            case "hr":
                            case "hrs":
                            case "h":      // Hours
                                dto = dto.AddHours(lastNumber);
                                break;
                            case "minute":
                            case "minutes":
                            case "min":
                            case "mins":
                            case "m":      // Minutes
                                dto = dto.AddMinutes(lastNumber);
                                break;
                        }
                    }
                }
                else lastNumber = curNumber;
            }

            return dto;
        }

        protected override void Setup(DiscordClient client) { }
    }
}
