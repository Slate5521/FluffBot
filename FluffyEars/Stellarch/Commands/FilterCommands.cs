// FilterCommands.cs
// Contains commands for the filter system:
//  !filter add/remove/list
//  !exclude add/remove/list
//
// (\-/)
//(='.'=)
//(")-(")o
// EMIKO

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using BigSister.ChatObjects;
using BigSister.Filter;

namespace BigSister.Commands
{
    [Group("Filter")]
    class FilterCommands : BaseCommandModule
    {
        [Command("filter"), 
            MinimumRole(Role.Mod),
            Description("Depending on user input, adds a new mask, removes a mask, or lists all the masks.\n\n" +
            "**Usage:**\n" +
            "!filter new/add <regexString>\ne.g. !filter new b[aeiou]nny" +
            "!filter remove/delete <regexString>\ne.g. !filter remove b[aeiou]nny" +
            "!filter list")]
        public async Task FilterBase(CommandContext ctx, string action, [RemainingText] string mask)
        {
            // Check if they have the permissions to call this command.
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                switch (action)
                {
                    case "new":
                    case "add":
                        if (!(await FilterSystem.HasItem(mask)))
                        {   // The mask doesn't exist already.
                            await FilterSystem.AddMask(ctx, mask);
                        } 
                        else
                        {   // The mask does exist.
                            await GenericResponses.SendGenericCommandError(
                                    ctx.Channel, 
                                    ctx.Member.Mention, 
                                    "Unable to add mask", 
                                    $"the provided mask `{mask}` exists already as an exclude or mask...");
                        }
                        break;
                    case "remove":
                    case "delete":
                        if((await FilterSystem.HasMask(mask)))
                        {   // The mask exists.
                            await FilterSystem.RemoveMask(ctx, mask);
                        }
                        else
                        {   // The mask doesn't exist.
                            await GenericResponses.SendGenericCommandError(
                                    ctx.Channel,
                                    ctx.Member.Mention,
                                    "Unable to remove mask",
                                    $"the provided mask `{mask}` does not exist...");
                        }
                        break;
                    case "list":
                        await FilterSystem.ListMasks(ctx);
                        break;
                    default: // Invalid arguments. 
                        await GenericResponses.HandleInvalidArguments(ctx);
                        break;
                }
            }
        }

        #region Excludes

        [Command("exclude"), 
            Aliases("excludes"), 
            MinimumRole(Role.Mod),
            Description("Depending on user input, adds a new exclude, removes an exclude, or lists all the excludes.\n\n" +
            "**Usage:**\n" +
            "!exclude(s) new/add <excludePhrase>\n*e.g. !exclude new bunnyes cute*" +
            "!exclude(s) remove/delete <excludePhrase>\n*e.g. !exclude remove bunnyes cute*" +
            "!exclude(s) list")]
        public async Task ExcludeBase(CommandContext ctx, string action, [RemainingText] string exclude)
        {
            // Check if they have the permissions to call this command.
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                switch (action)
                {
                    case "new":
                    case "add":
                        if (!(await FilterSystem.HasItem(exclude)))
                        {   // The exclude doesn't exist already.
                            await FilterSystem.AddExclude(ctx, exclude);
                        }
                        else
                        {   // The exclude does exist.
                            await GenericResponses.SendGenericCommandError(
                                    ctx.Channel,
                                    ctx.Member.Mention,
                                    "Unable to add exclude",
                                    $"the provided exclude `{exclude}` exists already as an exclude or mask...");
                        }
                        break;
                    case "remove":
                    case "delete":
                        if ((await FilterSystem.HasExclude(exclude)))
                        {   // The exclude  exists.
                            await FilterSystem.RemoveExclude(ctx, exclude);
                        }
                        else
                        {   // The exclude doesn't exist.
                            await GenericResponses.SendGenericCommandError(
                                    ctx.Channel,
                                    ctx.Member.Mention,
                                    "Unable to remove exclude",
                                    $"the provided exclude `{exclude}` exists already...");
                        }
                        break;
                    case "list":
                        await FilterSystem.ListExcludes(ctx);
                        break;
                    default:
                        await GenericResponses.HandleInvalidArguments(ctx);
                        break;
                }
            }
        }

        #endregion Excludes
    }
}
