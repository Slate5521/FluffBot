using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluffyEars.Commands
{
    class HelpCommand : BaseModule
    {
        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            if (ctx.Member.GetRole().IsCHOrHigher())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("```Bad words```");
                sb.AppendLine("__+badwords, +badword__");
                sb.AppendLine("[SEN.MOD+] Adds a single bad word or multiple bad words to the bad word list.\n**Usage:** +badword badword badword1 badword2 ... badwordn");
                sb.AppendLine("--------------------");

                sb.AppendLine("__-badwords, -badword__");
                sb.AppendLine("[SEN.MOD+] Removes a single or multiple bad words from the bad word list.\n**Usage:** -badword badword badword1 badword2 ... badwordn");
                sb.AppendLine("--------------------");

                sb.AppendLine("__listbadwords__");
                sb.AppendLine("[MOD+] Lists all the bad words currently being watched for.");
                sb.AppendLine("--------------------");


                sb.AppendLine("```Configuration```");
                sb.AppendLine("__setfilterchan, setfilterchannel, setfilter__");
                sb.AppendLine("[OWNER/ADMIN/BOT.BOI] Sets the channel the bot will record bad words into.\n**Usage:** setfilterchan #DiscordChannel");
                sb.AppendLine("--------------------");

                sb.AppendLine("__+whitelist__");
                sb.AppendLine("[OWNER] Whitelist a specific user.\n**Usage:** +whitelist @DiscordUser");
                sb.AppendLine("--------------------");

                sb.AppendLine("__-whitelist__");
                sb.AppendLine("[OWNER] Remove a specific user from the whitelist.\n**Usage:** -whitelist @DiscordUser");
                sb.AppendLine("--------------------");

                sb.AppendLine("__+chan, +channel__");
                sb.AppendLine("[OWNER/ADMIN/BOT.BOI] Un-exclude a channel from bad word searching.\n**Usage:** +chan #DiscordChannel");
                sb.AppendLine("--------------------");

                sb.AppendLine("__-chan, channel__");
                sb.AppendLine("[OWNER/ADMIN/BOT.BOI] Exclude a channel from bad word searching.\n**Usage:** -chan #DiscordChannel");
                sb.AppendLine("--------------------");

                sb.AppendLine("__listexcludes__");
                sb.AppendLine("List excluded channels.");
                sb.AppendLine("--------------------");


                sb.AppendLine("```Reminders```");

                sb.AppendLine("__+reminder__");
                sb.AppendLine("[CH+] Add a reminder.\n**Usage:** +reminder \\`time [month(s), week(s) day(s), hour(s), minute(s)\\` \\`reminder message\\` @mention1 @mention2 ... @mention_n\n<https://i.imgur.com/H1fVPta.png>");
                sb.AppendLine("--------------------");
                    
                sb.AppendLine("__-reminder__");
                sb.AppendLine("[CH+] Removes a reminder.\n**Usage:** -reminder reminder_id");
                sb.AppendLine("--------------------");

                sb.AppendLine("__listreminders__");
                sb.AppendLine("[CH+] Lists all pending notifications.");
                sb.AppendLine("--------------------");

                await ctx.Member.SendMessageAsync(sb.ToString());
            }
        }

        protected override void Setup(DiscordClient client) { }
    }
}
