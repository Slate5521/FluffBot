using BigSister.Reminders;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
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
                        if(await ReminderSystem.IsReminder(args))
                        {   // It's a reminder.
                            await ReminderSystem.RemoveReminder(ctx, args);
                        }
                        else
                        {   // It's not a reminder.
                            // Todo: respond
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
