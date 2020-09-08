using BigSister.ChatObjects;
using BigSister.Commands;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigSister
{
    [Flags]
    public enum Role
    {
        None = 0,
        Colonist = 1,
        Troubleshooter = 2,
        CS = 4,
        Mod = 8,
        SeniorMod = 16,
        BotManager = 32,
        Admin = 64
    }

    public static class Permissions
    {
        /// <summary>Handle checking permissions for a command.</summary>
        /// <returns>True if the user has the permissions to run a command.</returns>
        public static async Task<bool> HandlePermissionsCheck(CommandContext ctx)
        {
            bool pass_returnVal;

            var userPerms = new UserPermissions(ctx.Member.Roles);

            // Let's make sure they're not muted or have no role first of all.
            if (userPerms.Muted || userPerms.UserRoles == Role.None)
            {   // They are muted or have no roles.
                pass_returnVal = false;
            }
            else
            {   // They are not muted and do have roles, so let's continue.
                bool userHasPerms;
                MinimumRole minRole;

                // Let's make sure this isn't null.
                if (!(ctx.Command.CustomAttributes is null))
                {
                    var linq = ctx.Command.CustomAttributes.OfType<MinimumRole>();

                    // Let's make sure there's actually the attribute we want.
                    if (linq.Count() > 0)
                    {   // The attribute we want is here. 
                        minRole = linq.First();

                        userHasPerms = userPerms.IsRoleOrHigher(minRole.MinRole);

                        pass_returnVal = userHasPerms;
                    }
                    else
                    {   // It's not there for some reason, so let's throw an exception.
                        throw new Exception("Unable to load MinimumRole attribute.");
                    }
                }
                else
                {   // It's null, so let's throw an exception.
                    throw new Exception("Unable to load MinimumRole attribute.");
                }

                if(!pass_returnVal)
                {   // If they didn't pass but aren't muted and they do have roles, let's handle a response.
                    await HandleRejection(ctx, userPerms, minRole.MinRole);
                }
            }

            return pass_returnVal;
        }

        /// <summary>Handle a rejection that occurs due to insufficient permissions.</summary>
        public static async Task HandleRejection(CommandContext ctx, UserPermissions userPerms, Role minRole)
        {
            await ctx.Channel.SendMessageAsync(
                embed: GenericResponses.GetMessageInsufficientPermissions
                (
                    mention: ctx.Member.Mention,
                    minRole: UserPermissions.GetRoleString(minRole),
                    command: ctx.Command.Name
                ));
        }
    }

    public class UserPermissions
    {
        public readonly Role UserRoles;
        public readonly bool Muted;

        public UserPermissions(IEnumerable<DiscordRole> roles)
        {
            UserRoles = GetRoles(roles, out Muted);
        }

        /// <summary>Checks if the user is the desired role or higher.</summary>
        /// <param name="desiredRole">Desired role.</param>
        public bool IsRoleOrHigher(Role desiredRole)
            => UserRoles >= desiredRole;

        /// <summary>Get all the roles the user has.</summary>
        /// <returns>A [Flag] Role containing the roles of the user.</returns>
        public static Role GetRoles(IEnumerable<DiscordRole> roles, out bool muted)
        {
            Role role_returnVal = Role.None;
            muted = false;

            foreach (DiscordRole role in roles)
            {
                switch (role.Id)
                {
                    // ----------------------------------------------
                    case 409968304531439617: // Rimworld Colonist  
                    case 750984313813598218: // Dev Server Colonist

                        role_returnVal |= Role.Colonist;
                        break;
                    // ----------------------------------------------
                    case 329295632395272203: // Rimworld Troubleshooter
                    case 750985581701496872: // Dev Server Troubleshooter

                        role_returnVal |= Role.Troubleshooter;
                        break;
                    // ----------------------------------------------
                    case 326891962697383936: // Rimworld Community Support
                    case 673765694982193173: // Dev Server Comminuity Support

                        role_returnVal |= Role.CS;
                        break;
                    // ----------------------------------------------
                    case 214527027112312834: // Rimworld Moderator
                    case 690206811961294934: // Rimworld . Role 
                    case 673765713600708672: // Dev Server Moderator

                        role_returnVal |= Role.Mod;
                        break;
                    // ----------------------------------------------
                    case 521006886451937310: // Rimworld Senior Moderator
                    case 673765727605358614: // Dev Server Senior Moderator

                        role_returnVal |= Role.SeniorMod;
                        break;
                    // ----------------------------------------------
                    case 503752769757511690: // Rimworld Bot Manager
                    case 673765760656605194: // Dev Server Bot Manager

                        role_returnVal |= Role.BotManager;
                        break;
                    // ----------------------------------------------
                    case 214524811433607168: // Rimworld Admin 
                    case 673765748514095115: // Dev Server Admin

                        role_returnVal |= Role.Admin;
                        break;
                    // ----------------------------------------------
                    case 261282907149172737: // Rimworld Muted
                    case 750984286831902771: // Dev Server Muted

                        muted = true;
                        break;
                }
            }

            return role_returnVal;
        }

        /// <summary>Turn a Role into a user-friendly string.</summary>
        public static string GetRoleString(Role role)
        {
            string roleString_returnVal;

            switch(role)
            {
                default:
                    roleString_returnVal = String.Empty;
                    break;
                case Role.Colonist:
                    roleString_returnVal = @"Colonist";
                    break;
                case Role.Troubleshooter:
                    roleString_returnVal = @"Troubleshooter";
                    break;
                case Role.CS:
                    roleString_returnVal = @"Community Support";
                    break;
                case Role.Mod:
                    roleString_returnVal = @"Moderator";
                    break;
                case Role.SeniorMod:
                    roleString_returnVal = @"Senior Moderator";
                    break;
                case Role.BotManager:
                    roleString_returnVal = @"Bot Manager";
                    break;
                case Role.Admin:
                    roleString_returnVal = @"Admin";
                    break;
            }

            return roleString_returnVal;
        }
    }
}
