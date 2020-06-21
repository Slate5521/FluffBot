// WarnCommands.cs
// Contains commands for searching user warns. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace FluffyEars.Commands
{
    class WarnCommands : BaseCommandModule
    {

        public WarnCommands() { }

        [Command("setactionchan")]
        public async Task SetWarnChannel(CommandContext ctx, DiscordChannel chan)
        {
            throw new NotImplementedException();
        }

        [Command("setactionthreshold")]
        public async Task SetWarnThreshold(CommandContext ctx, ulong milliseconds)
        {
            throw new NotImplementedException();
        }
    }
}
