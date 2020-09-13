using BigSister.ChatObjects;
using BigSister.Reminders;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BigSister.Commands
{
    class ReminderCommands : BaseCommandModule
    {
        [Command("reminder"),
         Aliases("reminders"),
         MinimumRole(Role.Mod),
         Description("WIP")]
        public async Task ReminderBase(CommandContext ctx, string action, [RemainingText] string args)
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
