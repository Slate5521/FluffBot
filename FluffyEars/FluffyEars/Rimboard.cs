// Rimboard.cs
// If enough reactions are given to a message, pin them!

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using DSharpPlus;

namespace FluffyEars
{
    public static class Rimboard
    {
        internal static async Task BotClient_MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (BotSettings.RimboardEnabled && !e.User.IsCurrent)
            {
                var botPinnedEmoji = DiscordEmoji.FromName(Bot.BotClient, @":star:");


                // We don't want the cached version of this message because if it was sent during downtime, the bot won't be able to do
                // anything with it.
                var message_no_cache = await e.Channel.GetMessageAsync(e.Message.Id);

                // This contains a list of the reactions that have rimboardEmoji. It's only ever really going to be be 1 long.
                var pinReactions = message_no_cache.Reactions.Where(a => a.Emoji.GetDiscordName().Equals(BotSettings.RimboardEmoji));
                // Same thing as above, but it's looking for the botPinnedEmoji
                var botPinnedReaction = message_no_cache.Reactions.Where(a => a.Emoji.Equals(botPinnedEmoji));

                if (!(botPinnedReaction.Count() > 0 && botPinnedReaction.Any(a => a.IsMe)) &&
                    pinReactions.Count() > 0 && pinReactions.Any(a => a.Count >= BotSettings.RimboardThreshold))
                {   // Only continue if this hasn't been reacted to by the bot and it has more than a specified amount of emojis.
                    

                    await message_no_cache.DeleteAllReactionsAsync();
                    await message_no_cache.CreateReactionAsync(botPinnedEmoji);

                    var rimboardChannel = await Bot.BotClient.GetChannelAsync(BotSettings.RimboardChannelId);


                    if (e.Channel.Id.Equals(BotSettings.RimboardChannelId))
                    {   // If this is in our rimboard channel, let's pin it.

                        var pinnedMessages = await rimboardChannel.GetPinnedMessagesAsync();

                        if (pinnedMessages.Count == 50)
                        {
                            await pinnedMessages.Last().UnpinAsync();
                        }

                        await message_no_cache.PinAsync();
                    }
                    else
                    {   // Otherwise let's just add it.

                        // DEB!
                        var deb = new DiscordEmbedBuilder();


                        deb.WithColor(DiscordColor.Gold);
                        deb.WithDescription(message_no_cache.Content);
                        deb.WithThumbnail(message_no_cache.Author.AvatarUrl);

                        deb.AddField(@"Colonist", message_no_cache.Author.Mention, true);
                        deb.AddField(@"Link", Formatter.MaskedUrl($"#{message_no_cache.Channel.Name}", new Uri(ChatObjects.GetMessageUrl(message_no_cache))), true);

                        if (message_no_cache.Attachments.Count > 0)
                        {
                            deb.WithImageUrl(message_no_cache.Attachments[0].Url);
                        }

                        await rimboardChannel.SendMessageAsync(embed: deb);

                        if(message_no_cache.Embeds.Count > 0)
                        {
                            await rimboardChannel.SendMessageAsync(embed: message_no_cache.Embeds[0]);
                        }
                        
                    }
                }
            }
        }

    }
}
