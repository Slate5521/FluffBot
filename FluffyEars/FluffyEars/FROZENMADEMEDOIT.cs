using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//|￣￣￣￣￣￣￣￣|
//|     FROZEN    |
//|    MADE ME    | 
//|     DO IT     |
//| ＿＿＿＿＿＿＿| 
//(\__/) || 
//(•ㅅ•) || 
/// 　 づ
/// 　 /
/// 　 I SWEAR!


namespace FluffyEars
{
    static class FROZEN
    {
        static string[] frozenWords = new string[]
        {
            "frozen",
            "frozo",
            "forza",
            "freezy"
        };

        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            // AND WITH THIS, FROZEN'S OMNIPOTENCE IS ASSURED!

            if (!e.Author.IsBot || frozenWords.Any(e.Message.Content.ToLower().Contains))
            {
                DiscordChannel chan = await Bot.BotClient.GetChannelAsync(679933620034600960);
                await chan.SendMessageAsync(String.Format("<@113829933071073287> Someone mentioned you!\n{0}",
                    String.Format("https://discordapp.com/channels/{0}/{1}/{2}", e.Channel.GuildId, e.Channel.Id, e.Message.Id)));
                await chan.SendMessageAsync(e.Message.Content);
            }

        }
    }
}
