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
using DSharpPlus.Net;
using System.IO;
using System.Net;

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

                        // Let's check if it's an informational embed first.
                        if(message_no_cache.Content.Length == 0)
                        {   // It is an informational embed, most likely.
                            
                            var infoEmbed = message_no_cache.Embeds[0];

                            if(infoEmbed.Footer.Text.Length > 0)
                            {   // Let's check its footer for the real message.

                                if(ulong.TryParse(infoEmbed.Footer.Text, out ulong snowflake))
                                {
                                    DiscordMessage backup = message_no_cache;

                                    try
                                    {
                                        message_no_cache = await message_no_cache.Channel.GetMessageAsync(snowflake);
                                    } catch
                                    {
                                        message_no_cache = backup;
                                    }
                                }
                            }
                        }

                        var pinnedMessages = await rimboardChannel.GetPinnedMessagesAsync();

                        if (pinnedMessages.Count == 50)
                        {
                            await pinnedMessages.Last().UnpinAsync();
                        }

                        await message_no_cache.PinAsync();
                    }
                    else
                    {   // Otherwise let's just add it.

                        bool file = false;

                        // DEB!
                        var deb = new DiscordEmbedBuilder();

                        deb.WithColor(DiscordColor.Gold);

                        UriBuilder avatarUri = new UriBuilder(message_no_cache.Author.AvatarUrl);
                        avatarUri.Query = "?size=64";

                        deb.WithThumbnail(avatarUri.ToString());
                        deb.WithDescription(message_no_cache.Content);
                        deb.AddField(@"Colonist", $"{message_no_cache.Author.Mention}", true);
                        deb.AddField(@"Link", $"{Formatter.MaskedUrl($"#{message_no_cache.Channel.Name}", new Uri(ChatObjects.GetMessageUrl(message_no_cache)))}", true);

                        if (message_no_cache.Attachments.Count > 0)
                        {
                            file = true;
                        }

                        // Let's send this shit already.
                        //await Bot.SendWebhookMessage(message_no_cache.Content, new DiscordEmbed[] { deb.Build() });


                        if (file)
                        {   // Send a message with a file.
                            using(WebClient webclient = new WebClient())
                            {
                                string fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), 
                                    Path.GetExtension(message_no_cache.Attachments[0].FileName));

                                await webclient.DownloadFileTaskAsync(new Uri(message_no_cache.Attachments[0].Url), fileName);

                                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                                {
                                    // Send the file paired with the embed!
                                    await rimboardChannel.SendFileAsync(fs, embed: deb);
                                }

                                if(File.Exists(fileName))
                                {   // Delete it now that we're done with it.
                                    File.Delete(fileName);
                                } // end if
                            } // end using
                        }
                        else
                        {   // Send a message with no file.
                            await rimboardChannel
                                .SendMessageAsync(embed: deb);
                        }

                    } // end else
                } // end if
            } // end if
        } // end method

        private static void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
