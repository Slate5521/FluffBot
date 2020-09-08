using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using BigSister.ChatObjects;
using BigSister.Filter;

namespace BigSister.Commands
{
    class FilterCommands : BaseCommandModule
    {
        [Command("filter"),
         MinimumRole(Role.Mod),
         Description("WIP")]
        public async Task Filter(CommandContext ctx, string action, [RemainingText] string mask)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                switch (action)
                {
                    case "add":
                        if (!(await FilterSystem.HasMask(mask)))
                        {   // The mask doesn't exist already.
                            await FilterSystem.AddMask(ctx, mask);
                        } 
                        else
                        {   // The mask does exist.
                            //await GenericResponses.HandleGenericCommandError(ctx, $"the provided mask `{mask}` exists already...");
                        }
                        break;
                    case "remove":
                        if((await FilterSystem.HasMask(mask)))
                        {   // The mask exists.
                            await FilterSystem.RemoveMask(ctx, mask);
                        }
                        else
                        {   // The mask doesn't exist.
                            //await GenericResponses.HandleGenericCommandError(ctx, $"the provided mask `{mask}` does not exist already...");
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
         Description("WIP")]
        public async Task AddExclude(CommandContext ctx, string action, [RemainingText] string exclude)
        {
            switch(action)
            {
                case "add":
                    if (!(await FilterSystem.HasExclude(exclude)))
                    {   // The exclude doesn't exist already.
                        await FilterSystem.AddExclude(ctx, exclude);
                    }
                    else
                    {   // The exclude does exist.
                        //await GenericResponses.HandleGenericCommandError(ctx, $"the provided mask `{mask}` exists already...");
                    }
                    break;
                case "remove":
                    if ((await FilterSystem.HasExclude(exclude)))
                    {   // The exclude  exists.
                        await FilterSystem.RemoveExclude(ctx, exclude);
                    }
                    else
                    {   // The exclude doesn't exist.
                        //await GenericResponses.HandleGenericCommandError(ctx, $"the provided mask `{mask}` does not exist already...");
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

        #endregion Excludes
    }
}
