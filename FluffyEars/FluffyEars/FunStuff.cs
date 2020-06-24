// FunStuff.cs
// I put a lot of work into this bot, you know. I need to do some fun shit with it or what's the point of putting so much spirit into it.

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;

namespace FluffyEars
{
    internal class TriggerInfo
    {
        public DateTimeOffset LastTriggered { get; private set; }
        public TimeSpan CooldownTime { get; private set; }
        public Func<DiscordMessage, Task> Method { get; private set; }

        public TriggerInfo(TimeSpan cooldownTime, Func<DiscordMessage, Task> method)
        {
            CooldownTime = cooldownTime;
            Method = method;
        }

        public async Task AttemptTrigger(DiscordMessage message)
        {
            if (CanTrigger(message))
            {   // Only continue if this can be triggered.
                LastTriggered = message.CreationTimestamp.UtcDateTime;

                await Method(message);
            }
        }

        /// <summary>Check if this trigger can be triggered.</summary>
        public bool CanTrigger(DiscordMessage message)
        {   // Check if enough time has elapsed.

            long now = message.CreationTimestamp.UtcTicks;
            DateTimeOffset cooldownEnd = LastTriggered.Add(CooldownTime);

            return now >= cooldownEnd.UtcTicks;
        }
    }
    internal class FunMessage
    {
        public DiscordEmoji Emoji;
        public string Message;
        public TimeSpan MinTime = TimeSpan.FromMinutes(2);
        public TimeSpan MaxTime = TimeSpan.FromMinutes(4);
    }

    class FunStuff
    {
        static Dictionary<int, TriggerInfo> triggerDict = new Dictionary<int, TriggerInfo>();
        static List<FunMessage> funMessages = new List<FunMessage>
        {
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🐇"), Message = "Momu by Rekasa", MinTime = TimeSpan.FromMinutes(1), MaxTime = TimeSpan.FromMinutes(3)},
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🐇"), Message = "Momu by Rekasa", MinTime = TimeSpan.FromMinutes(1), MaxTime = TimeSpan.FromMinutes(3)},
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🐇"), Message = "Momu by Rekasa", MinTime = TimeSpan.FromMinutes(1), MaxTime = TimeSpan.FromMinutes(3)},  
            // Weight of 3x.

            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🧹"), Message = "Cleaning up the grounds.", MinTime = TimeSpan.FromMinutes(1), MaxTime = TimeSpan.FromMinutes(3) },
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🧹"), Message = "Cleaning up the grounds.", MinTime = TimeSpan.FromMinutes(1), MaxTime = TimeSpan.FromMinutes(3) }, 
            // Weight of 2x. 
            
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🗡️"), Message = "Taking care of raiders." },
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🎯"), Message = "Spending time in the rec room..." },
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"☁️"), Message = "Cloudwatching..." },
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🕒"), Message = "Wandering..." },
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"💀"), Message = "Disposing of raider bodies."},
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🥗"), Message = "Eating some vegetarian medley."},
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🔅"), Message = "Meditating."},
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"💓"), Message = "Healing after a raid."},
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🩺"), Message = "Healing a colonist."},
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"🔬"), Message = "....Researching....", MinTime = TimeSpan.FromMinutes(5), MaxTime= TimeSpan.FromMinutes(7)},
 
            new FunMessage { Emoji = DiscordEmoji.FromUnicode(@"😀"), Message = "Repopulating...", MinTime = TimeSpan.FromSeconds(15), MaxTime = TimeSpan.FromSeconds(40)},
        };
        static Random ran = new Random(Environment.TickCount);
        static DateTimeOffset NextTimeChange = DateTimeOffset.UtcNow;

        internal static async Task BotClient_MessageCreated(MessageCreateEventArgs e)
        {
            if(BotSettings.FunEnabled && !e.Channel.IsPrivate && !e.Author.IsCurrent)
            {   // Only continue if this isn't in private.
                var message = e.Message;

                TriggerInfo trigger;
                bool isTrigger = GetTriggerInfoFromMessage(message.Content, out trigger);

                if(isTrigger && trigger.CanTrigger(message))
                {   // Only do this if there's actually a trigger and if the bot can trigger.

                    await trigger.AttemptTrigger(message);
                } 
            }
        }

        internal static async Task BotClient_Heartbeated(HeartbeatEventArgs e)
        {
            if(BotSettings.FunEnabled)
            {
                if (DateTimeOffset.UtcNow.UtcTicks >= NextTimeChange.UtcTicks)
                {   // If it's change time, let's change.
                    FunMessage newStatus = funMessages[ran.Next(0, funMessages.Count - 1)];

                    try
                    {
                        await Bot.BotClient.UpdateStatusAsync
                            (
                                new DiscordActivity($"{newStatus.Emoji} {newStatus.Message}", ActivityType.Playing),
                                UserStatus.Online
                            );
                    } catch { }

                    // Update next time.
                    
                    TimeSpan nextUpdate = 
                        TimeSpan.FromMilliseconds(newStatus.MinTime.TotalMilliseconds + (ran.NextDouble() * newStatus.MaxTime.TotalMilliseconds));

                    NextTimeChange = NextTimeChange.Add(nextUpdate);
                }
            }
        }

        #region Regexes & Their Methods

        static RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled;

        static DiscordMessage OriginalResponseMessage;
        static DiscordUser YesNoLastUser;
        static string YesResponse;
        static string NoResponse;

        static Regex FloppyRegexYes = new Regex(@"^(?:yes|yea|yeah|ya)[.!?]?$", options);
        static Regex FloppyRegexNo = new Regex(@"^(?:no|nah)[.!?]?$", options);


        // --------------------------------
        // Yes / No Response

        private static async Task Yes(DiscordMessage message)
            => await YesNo(message, true);
        private static async Task No(DiscordMessage message)
            => await YesNo(message, false);

        private static async Task YesNo(DiscordMessage message, bool isYes)
        {
            if(!(YesNoLastUser is null) && message.Author.Id == YesNoLastUser.Id)
            {   // If there's a last user and the last user is equal to the sender...

                string response;

                if(isYes)
                {   // "Yes"
                    response = YesResponse;
                }
                else
                {   // "No"
                    response = NoResponse;
                }

                // Reset values before we respond.
                YesNoLastUser = null;
                YesResponse = null;
                NoResponse = null;
                if(!(OriginalResponseMessage is null))
                    await OriginalResponseMessage.DeleteAsync();
                OriginalResponseMessage = null;

                await message.Channel.SendMessageAsync(response);
            }
        }

        // --------------------------------
        // "I'm a bunny."

        static Regex ImBunnyRegex = new Regex(@"^(?:I'm|I am) a bunny[.!?]?$", options);
        private static async Task ImBunny(DiscordMessage message)
            => await message.RespondAsync(GenericResponse(@"Me too!"));

        // --------------------------------
        // "Floppy, you're cute."

        static Regex CuteFloppyRegex = new Regex(@"^Floppy,? (?:you're|you are) (?:cute|adorable|pretty|beautiful)[.!?]?$", options);
        private static async Task CuteFloppy(DiscordMessage message)
            => await message.RespondAsync(GenericResponse(@"Um.......................yeathanks", @"https://i.imgur.com/xe0tXL8.png"));

        // --------------------------------
        // Floppy, I love your ears.

        static Regex FloppyEarsRegex = new Regex(@"^Floppy,? (?:I love your ears|your ears are so long)[.!?]?$", options);
        private static async Task FloppyEars(DiscordMessage message)
            => await message.RespondAsync(GenericResponse(@"...whyareyoulookingatmyears?????...", @"https://i.imgur.com/5SK3Pwf.png"));

        // --------------------------------
        // :raid:

        static Regex FloppyRaidRegex = new Regex(@"^Floppy,? (?:we're|we are) being raided[.!?]?$", options);
        private static async Task FloppyRaid(DiscordMessage message)
            => await message.RespondAsync(GenericResponse(@"Target practice!", @"https://i.imgur.com/1EK1Oux.png"));

        // --------------------------------
        // Floppy carrot shit.
        static Regex FloppyCarrotRegex = new Regex(@"🥕", options);
        private static async Task FloppyCarrot(DiscordMessage message)
        {
            YesNoLastUser = message.Author;
            YesResponse = GenericResponse(@"Thanks!", @"https://i.imgur.com/iNyFJn5.png");
            NoResponse = GenericResponse(@"Whatever...");

            var reply = await message.Channel.SendMessageAsync(GenericResponse(@"...Can I have that carrot...?", @"https://i.imgur.com/uVm0FWs.png"));
            await SetOriginalResponseMessage(reply, false);
        }

        // --------------------------------
        // Floppy mask shit.

        static Regex FloppyMaskRegex = new Regex(@"^Floppy,? take off (?:that|your) mask[.!?]?$", options);
        private static async Task FloppyMask(DiscordMessage message)
        {
            YesNoLastUser = message.Author;
            YesResponse = GenericResponse(@"**H**ôŵ    *Þ*Ö  į  ł*ØÒ*k?*?*??", @"https://i.imgur.com/mCIgVVV.png");
            NoResponse  = GenericResponse(@"Maybe for the best...");

            var reply = await message.Channel.SendMessageAsync(GenericResponse(@"O -oh! A-are you sure?", @"https://i.imgur.com/V9ij2nN.png"));
            await SetOriginalResponseMessage(reply);
        }

        /// <summary>Set the original response message, deleting the previous instance if required.</summary>
        private static async Task SetOriginalResponseMessage(DiscordMessage message, bool deleteOld = true)
        {
            if (!(OriginalResponseMessage is null))
            {
                try
                {
                    await OriginalResponseMessage.DeleteAsync();
                } catch { }
            }

            if (deleteOld)
            {
                OriginalResponseMessage = message;
            }
        }

        private static string GenericResponse(string response, string url = null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(response);
            if (!(url is null))
            {
                stringBuilder.Append(' ');
                stringBuilder.Append(url);
            }

            return stringBuilder.ToString();
        }




        #endregion

        private static bool GetTriggerInfoFromMessage(string message, out TriggerInfo returnVal_trigger)
        {
            bool returnVal_success = true;

            if (ImBunnyRegex.IsMatch(message))
                returnVal_trigger = GetTriggerInfo(0, new TriggerInfo(cooldownTime: TimeSpan.FromMinutes(5), method: ImBunny));

            else if (CuteFloppyRegex.IsMatch(message))
                returnVal_trigger = GetTriggerInfo(1, new TriggerInfo(cooldownTime: TimeSpan.FromMinutes(5), method: CuteFloppy));

            else if (FloppyEarsRegex.IsMatch(message))
                returnVal_trigger = GetTriggerInfo(2, new TriggerInfo(cooldownTime: TimeSpan.FromMinutes(5), method: FloppyEars));

            else if (FloppyRaidRegex.IsMatch(message))
                returnVal_trigger = GetTriggerInfo(3, new TriggerInfo(cooldownTime: TimeSpan.FromMinutes(5), method: FloppyRaid));

            else if (FloppyMaskRegex.IsMatch(message))
                returnVal_trigger = GetTriggerInfo(4, new TriggerInfo(cooldownTime: TimeSpan.FromMinutes(5), method: FloppyMask));

            else if (FloppyCarrotRegex.IsMatch(message))
                returnVal_trigger = GetTriggerInfo(5, new TriggerInfo(cooldownTime: TimeSpan.FromMinutes(5), method: FloppyCarrot));

            else if (FloppyRegexYes.IsMatch(message))
                returnVal_trigger = GetTriggerInfo(6, new TriggerInfo(cooldownTime: TimeSpan.FromSeconds(1), method: Yes));

            else if (FloppyRegexNo.IsMatch(message))
                returnVal_trigger = GetTriggerInfo(7, new TriggerInfo(cooldownTime: TimeSpan.FromSeconds(1), method: No));

            else
            {
                returnVal_success = false;
                returnVal_trigger = default(TriggerInfo);
            }

            return returnVal_success;
        }

        private static TriggerInfo GetTriggerInfo(int i, TriggerInfo @default)
        {
            if(!triggerDict.ContainsKey(i))
            {
                triggerDict.Add(i, @default);
            }

            return triggerDict[i];
        }
    }
}
