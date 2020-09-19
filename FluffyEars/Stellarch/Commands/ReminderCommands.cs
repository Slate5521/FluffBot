// ReminderCommands.cs
// Contains methods for adding, removing, or listing reminders:
//  !reminder new/add <timeFrame> <message> <mentions> 
//  !reminder remove/delete <reminderId>
//  !reminder list
//
// EMIKO

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using BigSister.ChatObjects;
using BigSister.Reminders;

namespace BigSister.Commands
{
    class ReminderCommands : BaseCommandModule
    {
        [Command("reminder"),
         Aliases("reminders"),
         MinimumRole(Role.CS),
         Description("Add, remove, or list reminders.\n\n" +
            "**Example:** !reminder add 20d 4 mins unmute <@131626628211146752> thanks, also this is a valid message string\n\n" +
            "**Note:** Acceptable time formats are...\n\n" +
            "**Months:** month(s)\n" +
            "**Weeks:** week(s), wk(s), w\n" +
            "**Days:** day(s), d\n" +
            "**Hours:** hour(s), hr(s), h\n" +
            "**Minutes:** minute(s), min(s)\n")]
        public async Task ReminderBase(CommandContext ctx, 
                                       [Description("new/add, remove/delete, or list")]
                                            string action, 
                                       [RemainingText, Description("Time information, message, and mention(s) if applicable")] 
                                            string args)
        {
            // Check if they have the permissions to call this command.
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                switch (action)
                {
                    case "new":
                    case "add":
                        await ReminderSystem.AddReminder(ctx, args);
                        break;
                    case "remove":
                    case "delete":
                        // Check if this is even a reminder.

                        Reminder possibleReminder = await ReminderSystem.GetReminderFromDatabase(args);

                        if (!possibleReminder.Equals(Reminder.Invalid))
                        {   // It's a reminder.
                            await ReminderSystem.RemoveReminder(ctx, possibleReminder);
                        }
                        else
                        {   // It's not a reminder.
                            await GenericResponses.SendGenericCommandError(
                                    ctx.Channel,
                                    ctx.Member.Mention,
                                    "Unable to remove reminder",
                                    $"The reminder id `{args}` does not exist...");
                        }
                        break;
                    case "list":
                        await ReminderSystem.ListReminders(ctx);
                        break;
                }
            }
        }

    }
}
