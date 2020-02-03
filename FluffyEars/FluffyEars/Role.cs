using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluffyEars
{
    enum Role
    {
        Colonist = 0,
        CH = 1,
        Moderator = 2,
        SeniorModerator = 3,
        BotManager = 4,
        Admin = 5,
        Owner = 6
    }

    static class RoleExtensions
    {
        public static bool IsCHOrHigher(this Role role) => role >= Role.CH;
        public static bool IsModOrHigher(this Role role) => role >= Role.Moderator;
        public static bool IsSeniorModOrHigher(this Role role) => role >= Role.SeniorModerator;
        public static bool IsBotManagerOrHigher(this Role role) => role >= Role.BotManager;
        public static bool IsAdminOrHigher(this Role role) => role >= Role.Admin;
        public static bool IsOwner(this Role role) => role == Role.Owner;
        public static Role GetRole(this DiscordMember user)
        { 
            if(user.IsOwner)
                return Role.Owner;
          
            List<ulong> roles = user.Roles.Select(a => a.Id).ToList();

            if (roles.Contains(214524811433607168) || roles.Contains(673765748514095115)) // Admin
                return Role.Admin;
            if (roles.Contains(503752769757511690) || roles.Contains(673765760656605194)) // Bot Boi
                return Role.BotManager;
            if (roles.Contains(521006886451937310) || roles.Contains(673765727605358614)) // Senior Mod
                return Role.SeniorModerator;
            if (roles.Contains(214527027112312834) || roles.Contains(673765713600708672)) // Mod
                return Role.Moderator;
            if (roles.Contains(326891962697383936) || roles.Contains(673765694982193173)) // Community Helper
                return Role.CH;

            return Role.Colonist;
        }
    }
}
