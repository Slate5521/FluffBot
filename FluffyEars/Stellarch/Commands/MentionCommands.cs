// MentionCommands.cs
// Contains commands for looking up mentions:
//  !mentions <users>
//  !mentions <userIds>
//
//   (\_/)
//   (>.<)
//   (")_(")
//
// EMIKO

using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BigSister.ChatObjects;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace BigSister.Commands
{
    class MentionCommands : BaseCommandModule
    {
        static readonly Regex UserIdLookupRegex = 
            new Regex(@"(\d{17,})", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        [Command("mentions"), 
            MinimumRole(Role.CS), 
            Description("Looks for mention(s) of user(s) in the #action-logs channel.\n\n**Usage:** !mentions <user1 user2 ... usern>\n*e.g. !mentions 131626628211146752 <@131626628211146752>*\n\n**Note:** You can use a numerical User ID or a mention.")]
        public async Task MentionsLookup(CommandContext ctx, params string[] users)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                // Convert the string[] into a ulong[]

                ulong[] userIds = new ulong[users.Length];
                bool validArguments = true;

                for (int i = 0; i < users.Length && validArguments; i++)
                {
                    Match m = UserIdLookupRegex.Match(users[i]);

                    if (m.Success && ulong.TryParse(m.Value,
                                        out
                                        ulong a))
                    {
                        userIds[i] = a;
                    }
                    else
                    {
                        validArguments = false;
                    }
                }

                // Check if all arguments are valid. If any are not, we tell the user they fucked it up.
                if (validArguments)
                {
                    await MentionsLookupUnwrapper(ctx, userIds);
                } 
                else
                {
                    await GenericResponses.HandleInvalidArguments(ctx);
                }
            }
        }


        public async Task MentionsLookupUnwrapper(CommandContext ctx, ulong[] users)
            => await MentionSnooper.MentionSnooper.SeekWarns(ctx, users);
    }
}
