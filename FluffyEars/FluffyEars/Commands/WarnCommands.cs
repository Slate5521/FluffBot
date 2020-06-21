// WarnCommands.cs
// Contains commands for formatting warns and searching user warns. 
//
// I want to warn you, this is a bit of a mess. By no means does this follow any programming convention as entire bodies of methods seem to be copies
// of one another. The truth is that there was no real better way to do this in the 24 hours of time that I set out to complete it. Hopefully in a 
// future version I'll optimize this better, but in the mean time... sorry. Whoever you are, be it future me, or my predecessor, just know that if
// you want to change the body of one method, you will also have to likewise change the body of other methods. So, you'll want to die, but just bear
// through it.

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
        private enum ActionType
        {
            Invalid,
            Warn,
            Mute,
            Kick,
            Ban
        }

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

        static Regex SnowflakeRegex // This will find IDs of 17 length or longer (snowflakes are at minimum 17 digits).
            = new Regex(@"(\d{17,})", RegexOptions.Compiled);
        static Regex LinkRegex      // This will find links of the format http(s)://(www.)link.link/.../.../.../.../etc
            = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)", RegexOptions.Compiled);
        // Thanks to https://stackoverflow.com/questions/3809401/what-is-a-good-regular-expression-to-match-a-url

        [Command("action")]
        public async Task BeginTemplate(CommandContext ctx)
        {
            // Check if the user can use these sorts of commands.
            if (!ctx.Member.GetHighestRole().IsCSOrHigher())
            {
                await Bot.NotifyInvalidPermissions
                    (
                        requiredRole: Role.CS,
                        command: ctx.Command.Name,
                        channel: ctx.Channel,
                        caller: ctx.Member
                    );
            }
            else
            {
                const int TIMEOUT = 5;

                var interactivity = ctx.Client.GetInteractivity();

                List<ulong> mentionIds; // The mentioned ID(s).
                ActionType actionTaken; // Action taken by the staffmember.
                string reason;          // The reason for taking the action.
                List<string> proof;     // Screenshots or link proofs.

                // Let's send a "seperator"

                await ctx.Member.SendMessageAsync($"```NEW ACTION```");

                mentionIds = await GetIDs(TimeSpan.FromMinutes(TIMEOUT), interactivity, ctx.Member);
                actionTaken = await GetAction(TimeSpan.FromMinutes(TIMEOUT), interactivity, ctx.Member);
                reason = await GetReason(TimeSpan.FromMinutes(TIMEOUT), interactivity, ctx.Member);
                proof = await GetProof(TimeSpan.FromMinutes(TIMEOUT), interactivity, ctx.Member);
                
                var stringBuilder = new StringBuilder();

                // ----------------
                // Users

                stringBuilder.Append("**User");

                if(mentionIds.Count > 1)
                {   // More than one user.
                    stringBuilder.Append("s ");
                } else
                {   // No more than one other.
                    stringBuilder.Append(' ');
                }

                if(mentionIds.Count == 1)
                {   // One user.
                    stringBuilder.Append(ChatObjects.GetMention(mentionIds[0]));
                } 
                else if(mentionIds.Count == 2)
                {   // Two users.
                    stringBuilder.Append(ChatObjects.GetMention(mentionIds[0]));
                    stringBuilder.Append(@" and ");
                    stringBuilder.Append(ChatObjects.GetMention(mentionIds[1]));
                }
                else if(mentionIds.Count >= 3)
                {   // Three or more users
                    for(int i = 0; i < mentionIds.Count; i++)
                    {
                        if (i == mentionIds.Count - 1)
                        {   // If this is the last increment, throw an "and" in there.
                            stringBuilder.Append(@"and ");
                        }

                        stringBuilder.Append(ChatObjects.GetMention(mentionIds[i]));

                        if(i != mentionIds.Count - 1)
                        {   // So long as this is not the last index, then continue with appending commas.
                            stringBuilder.Append(@", ");
                        }
                    }
                }

                if(mentionIds.Count > 1)
                {   // More than one user
                    stringBuilder.Append(" were ");
                } 
                else
                {
                    stringBuilder.Append(" was ");
                }

                // ----------------
                // Action taken

                stringBuilder.Append($"{ActionVerb(actionTaken)} ");

                // ----------------
                // OP

                stringBuilder.Append($"by {ctx.Member.Mention} for: **");

                // ----------------
                // Reason

                stringBuilder.Append(reason);

                // ----------------
                // Links

                if(proof.Count > 0)
                {
                    stringBuilder.Append("\n\n**Proof:**\n");

                    int i = 0;
                    foreach(string link in proof)
                    {
                        stringBuilder.AppendLine($"`{++i}` - {link}");
                    }
                }

                // Wait for the user to react...

                string str = stringBuilder.ToString();

                await ctx.Member.SendMessageAsync(str);
                await Bot.NotifyActionChannel(str) ;
            }
        }

        [Command("searchactions")]
        public async Task BeginTemplate(CommandContext ctx, DiscordMember member)
        {
            throw new NotImplementedException();
        }

        /// <summary>Get the IDs of the action.</summary>
        private static async Task<List<ulong>> GetIDs(TimeSpan timeout, InteractivityExtension interactivity, DiscordMember originalSender)
        {
            // The list of IDs...
            List<ulong> ids = new List<ulong>();
            // True if more than one attempt.
            bool moreThanOnce = false;
            // True if the OP accepts this value.
            bool accepted = false;
            // Result of the original query
            InteractivityResult<DiscordMessage> result;
            // Cleanup function.

            do
            {   // User-acceptance checking loop
                do
                {   // Validity-checking loop
                    if (!moreThanOnce)
                    {
                        await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage("**Please enter the IDs or mentions of the user(s).**"));
                    }
                    else
                    {
                        await originalSender.SendMessageAsync(ChatObjects.GetErrMessage("**Invalid entry. Please enter the IDs or mentions of the user(s).**"));
                    }

                    result = await interactivity.WaitForMessageAsync(timeoutoverride: timeout, predicate: o =>
                    {
                        bool returnVal = true;

                        if (o.Author.Equals(originalSender) && o.Channel.IsPrivate)
                        {   // We only want to pay this message attention if it is sent from the original sender and is in a PM.

                            MatchCollection mc = SnowflakeRegex.Matches(o.Content);

                            foreach (Match match in mc)
                            {   // Let's go ahead and try to parse each match.
                                ulong @value; // The parsed value.

                                if (ulong.TryParse(match.Groups[1].Value, out @value))
                                {   // If it is successfully parsed, let's check it out to see if it's someone who exists.
                                    try
                                    {   // If this person isn't a real user, it'll throw a 404 at us.
                                        var user = interactivity.Client.GetUserAsync(@value).Result;

                                        ids.Add(@value);
                                    }
                                    catch { }

                                } // end if
                            } // end foreach
                        }  // end if
                        else
                        {   // We only want this to return as false in this case so we can ignore messages not sent in PM and not sent by the original
                            // caller.
                            returnVal = false;
                        }

                        return returnVal;
                    });

                    if (!moreThanOnce)
                    {   // If we reach the end of this while loop without evaluating, it definitely means there's been at least one attempt.
                        moreThanOnce = true;
                    }
                } while (ids.Count == 0 && !result.TimedOut); // Continue this loop until the list has something in it or until we've timed out. 

                // Let's see if the user agrees with this input.
                if (ids.Count > 0 && !result.TimedOut)
                {
                    // Get all the mentions
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendJoin(' ', ids.Select(o => ChatObjects.GetMention(o)));

                    await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage($"**Users selected:** {stringBuilder.ToString()} **Is this correct? (y/n)**"));

                    await interactivity.WaitForMessageAsync(timeoutoverride: timeout, predicate: o =>
                    {
                        string contentLwr = o.Content.ToLower(); // Lowercase version of the response.
                        bool validResponse = false; // If the response is valid (y/n)

                        if (o.Author.Equals(originalSender) && o.Channel.IsPrivate && !o.Author.IsBot)
                        {
                            if (contentLwr.Equals(@"y") || contentLwr.Equals(@"n"))
                            {   // We only want to enter this scope if the content is equal to y/n
                                validResponse = true;

                                accepted = contentLwr.Equals(@"y");
                            }
                        }

                        return validResponse;
                    });

                    if(!accepted && !result.TimedOut)
                    {   // Only enter if not accepted and not timed out.

                        // Clear attempts.
                        moreThanOnce = false;
                        // Clear response to default.
                        ids.Clear();

                        await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage("`Retrying...`"));
                    } //end if
                } // end if
            } while (!accepted); // Continue this loop so long as OP hasn't accepted the value.

            return ids;
        }

        /// <summary>Get the action's type.</summary>
        private static async Task<ActionType> GetAction(TimeSpan timeout, InteractivityExtension interactivity, DiscordMember originalSender)
        {
            // The action taken.
            var actionType = ActionType.Invalid;
            // True if more than one attempt.
            bool moreThanOnce = false;
            // True if the OP accepts this value.
            bool accepted = false;
            // Result of the original query
            InteractivityResult<DiscordMessage> result;
            // Cleanup function.
            List<DiscordMessage> cleanup = new List<DiscordMessage>();

            do
            {   // User-acceptance checking loop
                do
                {   // Validity-checking loop
                    if (!moreThanOnce)
                    {
                        cleanup.Add(await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage($"**Please enter the # of the action taken:**```\n{GetActionString()}```")));
                    }
                    else
                    {
                        cleanup.Add(await originalSender.SendMessageAsync(ChatObjects.GetErrMessage($"**Invalid entry. Please enter the # of the action taken:**```\n{GetActionString()}```")));
                    }

                    result = await interactivity.WaitForMessageAsync(timeoutoverride: timeout, predicate: o =>
                    {
                        bool returnVal = true;

                        if (o.Author.Equals(originalSender) && o.Channel.IsPrivate)
                        {   // We only want to pay this message attention if it is sent from the original sender and is in a PM.
                            char c;

                            if (o.Content.Length == 1 && char.TryParse(o.Content, out c))
                            {   // Only continue if it's both char-sized and a character.
                                actionType = GetActionTaken(c);
                            }
                        }
                        else
                        {   // We only want this to return as false in this case so we can ignore messages not sent in PM and not sent by the original
                            // caller.
                            returnVal = false;
                        }

                        return returnVal;
                    });

                    if (!moreThanOnce)
                    {   // If we reach the end of this while loop without evaluating, it definitely means there's been at least one attempt.
                        moreThanOnce = true;
                    }
                } while (actionType.Equals(ActionType.Invalid) && !result.TimedOut);
                // ^ Continue this loop until we have a valid response or until we've timed out. 

                // Let's see if the user agrees with this input.
                if (!actionType.Equals(ActionType.Invalid) && !result.TimedOut)
                {
                    cleanup.Add(await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage($"**Action selected:** {ActionVerb(actionType)} **Is this correct? (y/n)**")));

                    await interactivity.WaitForMessageAsync(timeoutoverride: timeout, predicate: o =>
                    {
                        string contentLwr = o.Content.ToLower(); // Lowercase version of the response.
                        bool validResponse = false; // If the response is valid (y/n)

                        if (o.Author.Equals(originalSender) && o.Channel.IsPrivate && !o.Author.IsBot)
                        {
                            if (contentLwr.Equals(@"y") || contentLwr.Equals(@"n"))
                            {   // We only want to enter this scope if the content is equal to y/n
                                validResponse = true;

                                accepted = contentLwr.Equals(@"y");
                            }
                        }

                        return validResponse;
                    });

                    if (!accepted)
                    {
                        // Clear attempts.
                        moreThanOnce = false;
                        // Clear response to default.
                        actionType = ActionType.Invalid;

                        cleanup.Add(await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage("`Retrying...`")));

                    } // end if
                } // end if
            } while (!accepted); // Continue this loop so long as OP hasn't accepted the value.

            return actionType;
        }

        /// <summary>Get the action's reason.</summary>
        private static async Task<string> GetReason(TimeSpan timeout, InteractivityExtension interactivity, DiscordMember originalSender)
        {
            // The reason the action was taken.
            string actionReason = String.Empty;
            // True if more than one attempt.
            bool moreThanOnce = false;
            // True if the OP accepts this value.
            bool accepted = false;
            // Result of the original query
            InteractivityResult<DiscordMessage> result;

            do
            {   // User-acceptance checking loop
                do
                {   // Validity-checking loop
                    if (!moreThanOnce)
                    {
                        await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage($"**Please enter the reason:**"));
                    }
                    else
                    {
                        await originalSender.SendMessageAsync(ChatObjects.GetErrMessage($"**Invalid entry. Please enter the reason:**"));
                    }

                    result = await interactivity.WaitForMessageAsync(timeoutoverride: timeout, predicate: o =>
                    {
                        bool returnVal = true;

                        if (o.Author.Equals(originalSender) && o.Channel.IsPrivate)
                        {   // We only want to pay this message attention if it is sent from the original sender and is in a PM.

                            if (o.Content.Length > 0)
                            {
                                actionReason = o.Content;
                            }
                        }
                        else
                        {   // We only want this to return as false in this case so we can ignore messages not sent in PM and not sent by the original
                            // caller.
                            returnVal = false;
                        }

                        return returnVal;
                    });

                    if (!moreThanOnce)
                    {   // If we reach the end of this while loop without evaluating, it definitely means there's been at least one attempt.
                        moreThanOnce = true;
                    }
                } while (actionReason.Length == 0 && !result.TimedOut);
                // ^ Continue this loop until we have a valid response or until we've timed out. 

                // Let's see if the user agrees with this input.
                if (actionReason.Length > 0 && !result.TimedOut)
                {
                    await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage($"**Reason provided:** {actionReason} **Is this correct? (y/n)**"));

                    await interactivity.WaitForMessageAsync(timeoutoverride: timeout, predicate: o =>
                    {
                        string contentLwr = o.Content.ToLower(); // Lowercase version of the response.
                        bool validResponse = false; // If the response is valid (y/n)


                        if (o.Author.Equals(originalSender) && o.Channel.IsPrivate && !o.Author.IsBot)
                        {
                            if (contentLwr.Equals(@"y") || contentLwr.Equals(@"n"))
                            {   // We only want to enter this scope if the content is equal to y/n
                                validResponse = true;

                                accepted = contentLwr.Equals(@"y");
                            }
                        }

                        return validResponse;
                    });

                    if (!accepted)
                    {
                        // Clear attempts.
                        moreThanOnce = false;
                        // Clear response to default.
                        actionReason = String.Empty;

                        await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage("`Retrying...`"));
                    } // end if
                } // end if
            } while (!accepted); // Continue this loop so long as OP hasn't accepted the value.
            
            return actionReason;
        }

        /// <summary>Get the action's proof.</summary>
        private static async Task<List<string>> GetProof(TimeSpan timeout, InteractivityExtension interactivity, DiscordMember originalSender)
        {
            const int MAX_LINKS = 10;

            // The list of links
            List<string> links = new List<string>();
            // We will continue this until the user tells us to stop or until we reach 10 links.
            bool haltSignal = false;
            // True if the OP accepts this value.
            bool accepted = false;
            // Result of the original query
            InteractivityResult<DiscordMessage> result;

            do
            {   // User-acceptance checking loop

                await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage($"**Please send proof in images or links. Max 10.** Send '__end__' to continue or if you don't want to provide proof."));
                
                do
                {   // Validity-checking loop
                    result = await interactivity.WaitForMessageAsync(timeoutoverride: timeout, predicate: o =>
                    {
                        bool returnVal = true;

                        if (o.Author.Equals(originalSender) && o.Channel.IsPrivate)
                        {   // We only want to pay this message attention if it is sent from the original sender and is in a PM.

                            if (o.Content.Equals(@"end"))
                            {   // If it's equal to our halt word, we need to stop.
                                haltSignal = true;
                            }
                            else
                            {   // No halt word found.

                                // Check for attachments first...

                                if (o.Attachments.Count > 0)
                                {
                                    foreach (var attachment in o.Attachments)
                                    {
                                        if (links.Count < MAX_LINKS && !links.Contains(attachment.Url))
                                        {   // Only continue if we're under the link limit and the links doesn't already contain this.
                                            links.Add(attachment.Url);
                                        } // end if
                                    } // end foreach
                                } // end if

                                // Next, check for links in the message.

                                MatchCollection mc = LinkRegex.Matches(o.Content);

                                if (mc.Count > 0)
                                {
                                    foreach (Match match in mc)
                                    {
                                        if (links.Count < MAX_LINKS && !links.Contains(match.Value))
                                        {   // Only continue if we're under the link limit and the links doesn't already contain this.
                                            links.Add(match.Value);
                                        } // end if
                                    } // end foreach
                                } // end if
                            } // end if
                        } // end if
                        else
                        {   // We only want this to return as false in this case so we can ignore messages not sent in PM and not sent by the original
                            // caller.
                            returnVal = false;
                        }

                        return returnVal;
                    });
                } while (!haltSignal && links.Count < MAX_LINKS && !result.TimedOut);
                // ^ Continue this loop until we receive a halt signal, we've reached 10 links, or until we've timed out.

                // Let's see if the user agrees with this input.
                if ((haltSignal || links.Count == MAX_LINKS) && !result.TimedOut)
                {   // Enter this scope if we've [(A) received the halt signal OR (B) hit max links] AND (C) haven't timed out.

                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendJoin('\n', links);

                    await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage($"**Links provided:\n\n** {stringBuilder.ToString()} **\n\nIs this correct? (y/n)**"));

                    await interactivity.WaitForMessageAsync(timeoutoverride: timeout, predicate: o =>
                    {
                        string contentLwr = o.Content.ToLower(); // Lowercase version of the response.
                        bool validResponse = false; // If the response is valid (y/n)

                        if (o.Author.Equals(originalSender) && o.Channel.IsPrivate && !o.Author.IsBot)
                        {
                            if (contentLwr.Equals(@"y") || contentLwr.Equals(@"n"))
                            {   // We only want to enter this scope if the content is equal to y/n
                                validResponse = true;

                                accepted = contentLwr.Equals(@"y");
                            }
                        }

                        return validResponse;
                    });

                    if (!accepted)
                    {
                        // Clear response to default.
                        haltSignal = false;
                        links.Clear();

                        await originalSender.SendMessageAsync(ChatObjects.GetNeutralMessage("`Retrying...`"));
                    } // end if
                } // end if
            } while (!accepted); // Continue this loop so long as OP hasn't accepted the value.
            
            return links;
        }

        /// <summary>Get all the available types of actions.</summary>
        private static string GetActionString()
        {
            return "1-Warn\n2-Mute\n3-Kick\n4-Ban";
        }

        /// <summary>Take the user-response (a number) and assign an action type to it.</summary>
        private static ActionType GetActionTaken(char index)
        {
            ActionType actionType;

            switch (index)
            {
                case '1': actionType = ActionType.Warn; break;
                case '2': actionType = ActionType.Mute; break;
                case '3': actionType = ActionType.Kick; break;
                case '4': actionType = ActionType.Ban; break;
                default: actionType = ActionType.Invalid; break;
            }

            return actionType;
        }

        /// <summary>Turn an ActionType enum into its verb.</summary>
        private static string ActionVerb(ActionType type)
        {
            string returnVal;

            switch (type)
            {
                case ActionType.Warn: returnVal = @"warned"; break;
                case ActionType.Mute: returnVal = @"muted"; break;
                case ActionType.Kick: returnVal = @"kicked"; break;
                case ActionType.Ban: returnVal = @"banned"; break;
                default:
                case ActionType.Invalid: returnVal = @"<invalid>"; break;
            }

            return returnVal;
        }
    }
}
