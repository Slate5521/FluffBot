// Role.cs
// For working with roles and their permissions.

using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluffyEars
{
    enum Role
    {
        Colonist = 0,
        CS = 1,
        Moderator = 2,
        SeniorModerator = 3,
        BotManager = 4,
        Admin = 5,
        Owner = 6
    }

    #region ... bunny!??

    /*
                                                        MMMMMMMMMMMMNmNMMMMMMMMMMMMMMMMMMMMMMMMMNmNMMMMMMMMMMMM
                                                        MMMMMMMMMMMho::odNMMMMMMMMMMMMMMMMMMMNdsssshMMMMMMMMMMM
                                                        MMMMMMMMMMdo`   -smMMMMMMMMMMMMMMMMMmyssssssdMMMMMMMMMM
                                                        MMMMMMMMMMs/     `omMMMMMMMMMMMMMMMms///+osssMMMMMMMMMM
                                                        MMMMMMMMMNs:      .smMMMMMMMMMMMMMms.     `+sNMMMMMMMMM
                                                        MMMMMMMMMNs:       -yMMMMMMMMMMMMMy-       :sNMMMMMMMMM
                                                        MMMMMMMMMMy/       `+dMMMMMMMMMMMd/        /yMMMMMMMMMM
                                                        MMMMMMMMMMho      .:/sMMNNNNNNNMMs`        ohMMMMMMMMMM
                                                        MMMMMMMMMMms.      ::+yyyyyyyyyyy/        .smMMMMMMMMMM
                                                        MMMMMMMMMMMh/        .sssssssssss.        /hMMMMMMMMMMM
                                                        MMMMMMMMMMMNs`        ossssssssso        `sNMMMMMMMMMMM
                                                        MMMMMMMMMMMMh+        ::-.....-::        +hMMMMMMMMMMMM
                                                        MMMMMMMMMMMmys-                         -symMMMMMMMMMMM
                                                        MMMMMMMMMMmssso`       `               `osssmMMMMMMMMMM
                                                        MMMMMMMMMNysso-       .+`               -ossyNMMMMMMMMM
                                                        MMMMMMMMMdsss.    `//:o-       `.-//`    .sssdMMMMMMMMM
                                                        MMMMMMMMMhss:     `osss+:     :+/.-+`     :sshMMMMMMMMM
                                                        MMMMMMMMMyss`     `osso:       :/.-+`     `ssyMMMMMMMMM
                                                        MMMMMMMMMhss    .  `:o`         `..`  .    sshMMMMMMMMM
                                                        MMMMMMMMMNss- ./:`- -+   `...`      -`:/. -ssNMMMMMMMMM
                                                        MMMMMMMMMMhso+/.-+- `.   :sss:      -+-./+oshMMMMMMMMMM
                                                        MMMMMMMMMMMhss+o:`      .//-//.      `:o+sshMMMMMMMMMMM
                                                        MMMMMMMMMMMMdssss+:-``  ``   ``  ``-:+ssssdMMMMMMMMMMMM
                                                        MMMMMMMMMMMMMmyssssssso+++///+++osssssssymMMMMMMMMMMMMM
                                                        MMMMMMMMMMMMMMMmhssssssssssssssssssssshmMMMMMMMMMMMMMMM
                                                        MMMMMMMMMMMMMMMMMNmhysssssssssssssyhmNMMMMMMMMMMMMMMMMM
                                                        MMMMMMMMMMMMMMMMMMMMMNNmdddddddmNNMMMMMMMMMMMMMMMMMMMMM
                                                        MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM
     */

    #endregion

    static class RoleExtensions
    {
        public static bool IsCSOrHigher(this Role role) 
            => role >= Role.CS;
        public static bool IsModOrHigher(this Role role) 
            => role >= Role.Moderator;
        public static bool IsSeniorModOrHigher(this Role role) 
            => role >= Role.SeniorModerator;
        public static bool IsBotManagerOrHigher(this Role role) 
            => role >= Role.BotManager;
        public static bool IsAdminOrHigher(this Role role) 
            => role >= Role.Admin;
        public static bool IsOwner(this Role role) 
            => role == Role.Owner;

        /// <summary>Get the highest ranking role the user has.</summary>
        public static Role GetHighestRole(this DiscordMember user)
        { 
            // This method is an absolute dumpster fire when it comes to SESE-based programming.

            if(user.Guild.Owner.Id.Equals(user.Id))
                return Role.Owner;
          
            var roles = user.Roles.Select(a => a.Id).ToList();

            //                 RIMWORLD ROLES      ||                DEV SERVER ROLES
            if (roles.Contains(214524811433607168) || roles.Contains(673765748514095115)) // Admin
                return Role.Admin;                                                     //||                Rimworld Dev Mode
            if (roles.Contains(503752769757511690) || roles.Contains(673765760656605194) || roles.Contains(653357224520974355)) // Bot Boi
                return Role.BotManager;
            if (roles.Contains(521006886451937310) || roles.Contains(673765727605358614)) // Senior Mod
                return Role.SeniorModerator;
            if (roles.Contains(214527027112312834) || roles.Contains(673765713600708672)) // Mod
                return Role.Moderator;
            if (roles.Contains(326891962697383936) || roles.Contains(673765694982193173)) // Community Helper
                return Role.CS;

            return Role.Colonist;
        }

        /// <summary>Turn a role enum value into a name.</summary>
        public static string ToName(this Role role)
        {
            string returnVal;

            switch(role)
            {
                case Role.Colonist:        returnVal = @"Colonist";          break;
                case Role.CS:              returnVal = @"Community Support"; break;
                case Role.Moderator:       returnVal = @"Moderator";         break;
                case Role.SeniorModerator: returnVal = "Senior Moderator";   break;
                case Role.BotManager:      returnVal = "Bot Manager";        break;
                case Role.Admin:           returnVal = "Admin";              break;
                case Role.Owner:           returnVal = "Server Owner";       break;
                default:                   returnVal = @"ERROR";             break;
            }

            return returnVal;
        }
    }
}
