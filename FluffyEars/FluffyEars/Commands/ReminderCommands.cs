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
        private bool CanUseReminderCommands(DiscordMember user) => true;//user.Roles.Any(a => IsAcceptedRole(a));

        private bool IsAcceptedRole(DiscordRole role)
        {
            return role.Id == 214524811433607168 || // Admin
                   role.Id == 503752769757511690 || // Bot Manager
                   role.Id == 521006886451937310 || // Senior Mod
                   role.Id == 214527027112312834 || // Mod
                   role.Id == 326891962697383936 || // CH
                   role.Id == 672663130324598811;   // FDS test role
        }


        [Command("addreminder")]
        public async Task AddReminder(CommandContext ctx)
        {
            if (CanUseReminderCommands(ctx.Member))
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

                    // Default message.
                    if (messageString.Trim() == String.Empty)
                        messageString = "Untitled notification";

                    // Check how many spaces there are. If there are none, that means there really isn't anything to take in.
                    if (timeString.Count(a => a == ' ') < 1)
                        await ctx.Channel.SendMessageAsync(@"Invalid time");
                    else
                    {
                        DateTimeOffset dto = InterpretTimeString(timeString);

                        // Generate a new reminder!
                        Reminder reminder = new Reminder
                        {
                            Text = messageString,
                            Time = dto.ToUnixTimeMilliseconds(),
                            User = ctx.Member.Id,
                            Channel = ctx.Channel.Id
                        };

                        deb.WithTitle(@"Notification");
                        deb.AddField(@"User", ctx.Member.Mention);
                        deb.AddField(@"Time", dto.ToString());
                        deb.AddField(@"Message", messageString);
                        deb.AddField(@"Notification Identifier", reminder.GetIdentifier());

                        ReminderSystem.AddReminder(reminder);

                        await ctx.Channel.SendMessageAsync(String.Empty, false, deb);
                        ReminderSystem.Save();
                    }
                }
                else
                    await ctx.Channel.SendMessageAsync(@"Unrecognized arguments.");

            }
        }

        [Command("removereminder")]
        public async Task RemoveReminder(CommandContext ctx, string reminderId)
        {
            // Only enter scope if (a) user can use reminder commands && this is an existing reminder.
            if(CanUseReminderCommands(ctx.Member) && ReminderSystem.IsReminder(reminderId))
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
                deb.AddField(@"Remaining time", remainingTime.ToString());
                deb.AddField(@"Message", reminderToRemove.Text);
                deb.AddField(@"Notification Identifier", reminderId);

                ReminderSystem.RemoveReminder(reminderToRemove);

                await ctx.Channel.SendMessageAsync(originalAuthorMention, false, deb);
                ReminderSystem.Save();
            }
        }

        [Command("listreminders")]
        public async Task ListReminders(CommandContext ctx)
        {
            if(CanUseReminderCommands(ctx.Member))
            {
                // Check if there are any notifications. If there are none, let the user know.
                if (ReminderSystem.HasNotification())
                {
                    // DEB!
                    DiscordEmbedBuilder deb = new DiscordEmbedBuilder();
                    deb.WithTitle("Reminder List");

                    // Get a list of reminders, but ordered descending by their remind date.
                    foreach(Reminder reminder in 
                        ReminderSystem.GetReminders().OrderByDescending(a => a.Time))
                    {
                        DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(reminder.Time);

                        deb.AddField(dto.ToString("ddMMMyyyy HH:mm"),
                            String.Format("<@{0}>: {1}\nId: {2}", reminder.User, reminder.Text, reminder.GetIdentifier()));
                    }

                    await ctx.Channel.SendMessageAsync(embed: deb);
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
