using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BigSister.ChatObjects
{
    public class WebhookInfo
    {
        public static WebhookInfo Invalid = new WebhookInfo();

        public ulong Id;

        bool validWebhook;
        DiscordWebhook webhook;
        
        public WebhookInfo() { validWebhook = false; }

        /// <summary>Generate a new webhook instance with the provided settings.</summary>
        public WebhookInfo(ulong id, string token)
        {
            webhook = Program.BotClient.GetWebhookWithTokenAsync(id, token)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            Id = id;

            validWebhook = true;
        }


        public async Task SendMessage(string content = null, 
                                      DiscordEmbed[] embeds = null)
        {
            var dwb = new DiscordWebhookBuilder();

            if (!(embeds is null))
            {
                if (embeds.Length > 10)
                {
                    throw new ArgumentException("More than 10 embeds provided.");
                }

                dwb.AddEmbeds(embeds);
            }

            if (!(content is null))
            {
                dwb.WithContent(content);
            }

            if (embeds is null && content is null)
            {
                throw new ArgumentException("Cannot send an empty message.");
            }

            await webhook.ExecuteAsync(dwb);
        }

        public bool Valid(this WebhookInfo info)
            => !Equals(Invalid);

        public override bool Equals(object obj)
        {
            return obj is WebhookInfo info &&
                   validWebhook == info.validWebhook &&
                   EqualityComparer<DiscordWebhook>.Default.Equals(webhook, info.webhook);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(validWebhook, webhook);
        }
    }
}
