// SettingsCommands.cs
// Contains commands to set options that are saved in the plaintext save file.

using BigSister.ChatObjects;
using BigSister.Settings;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BigSister.Commands
{
    class SettingsCommands : BaseCommandModule
    {
        const Role ConfigPerm = Role.BotManager;

        private string GetEnabledDisabled(bool a)
            => a ? @"enabled" : @"disabled";

        #region Filter

        [Command("excluded-channels"), Aliases("excluded-chans"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task ExcludeChannel(CommandContext ctx, string command, params DiscordChannel[] channels)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Func<DiscordChannel, bool> commandsAction;
                string verb;
                string verb2;

                switch (command)
                {
                    case "add":
                        commandsAction = delegate (DiscordChannel c)
                        {   ulong id = c.Id;
                            // Make sure it doesn't exist.
                            bool notExists_returnVal = !Program.Settings.ExcludedChannels.Contains(id);
                            // It doesn't exist, so we can add it. 
                            if(notExists_returnVal) 
                                Program.Settings.ExcludedChannels.Add(id);

                            return notExists_returnVal;
                        };

                        verb = @"add";
                        verb2 = @"added";
                        break;
                    case "remove":
                        commandsAction = delegate (DiscordChannel c)
                        {   ulong id = c.Id;
                            // Check if it exists.
                            bool exists_returnVal = !Program.Settings.ExcludedChannels.Contains(id);
                            // Only remove it if it exists.
                            if (exists_returnVal)
                                Program.Settings.ExcludedChannels.Remove(id);

                            return exists_returnVal;
                        };

                        verb = @"remove";
                        verb2 = @"removed";
                        break;
                    default: 
                        commandsAction = null;
                        verb = String.Empty;
                        verb2 = String.Empty;
                        break;
                }

                // Check if we got a valid command arg.
                if(commandsAction is null)
                {   // We did not get a valid command arg.
                    await GenericResponses.HandleInvalidArguments(ctx);
                }
                else
                {   // We did get a valid command arg.
                    // Array of everything and a description of if it was added or not.
                    Dictionary<DiscordChannel, bool> successAdded =
                        new Dictionary<DiscordChannel, bool>(channels.Length);

                    // Loop through each channel.
                    foreach(DiscordChannel chan in channels)
                    {   // Invoke our command on each channel
                        successAdded.Add(chan, commandsAction.Invoke(chan));
                    }

                    // Check if anything was added successfully.
                    if(successAdded.Any(a => a.Value))
                    {   // Yes, at least one thing was added successfully. So let's save the settings now that they've been updated.
                        Program.SaveSettings();
                    }

                    // Respond.

                    await GenericResponses.SendMessageChangedNotChanged(
                        channel: ctx.Channel,
                        title: $"Attempted to {verb} channel(s) to exclusion",
                        mention: ctx.Member.Mention, 
                        body: $"I attempted to {verb} the channels you gave me.", 
                        successChanged: successAdded, 
                        verb: verb2, 
                        invertedVerb: $"not {verb2}");
                }

            }
        }

        [Command("filter-channel"), Aliases("filter-chan"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task ExcludeChannel(CommandContext ctx, DiscordChannel channel)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Program.UpdateSettings(ref Program.Settings.FilterChannelId, channel.Id);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Filter channel changed",
                    valueName: @"filter channel",
                    newVal: channel.Mention);
            }
        }

        #endregion Filter
        #region Reminders

        [Command("reminder-limit"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task SetReminderTimeLimit(CommandContext ctx, uint months)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                // Clamp it to the max value first so the bot doesn't crash.
                int monthsInt = (int)Math.Min(months, int.MaxValue);

                Program.UpdateSettings(ref Program.Settings.MaxReminderTimeMonths, monthsInt);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Max reminder duration changed",
                    valueName: @"max reminder duration",
                    newVal: monthsInt.ToString());
            }
        }

        #endregion Reminders
        #region Warn Snooper

        [Command("warn-month-limit"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task SetMaxWarnLimit(CommandContext ctx, uint months)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                // Clamp it to the max value first so the bot doesn't crash.
                int monthsInt = (int)Math.Min(months, int.MaxValue);

                Program.UpdateSettings(ref Program.Settings.MaxActionAgeMonths, monthsInt);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Warning threshold changed",
                    valueName: @"warn month limit",
                    newVal: monthsInt.ToString());
            }
        }

        [Command("action-channel"), Aliases("action-chan"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task SetActionChannel(CommandContext ctx, DiscordChannel channel)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Program.UpdateSettings(ref Program.Settings.ActionChannelId, channel.Id);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Action channel changed",
                    valueName: @"action channel",
                    newVal: channel.Mention);
            }
        }

        [Command("snoop-enabled"), Aliases("warnsnoopenabled", "snoopdogenabled"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task SetActionChannel(CommandContext ctx, bool enabled)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Program.UpdateSettings(ref Program.Settings.AutoWarnSnoopEnabled, enabled);

                string a = GetEnabledDisabled(enabled);
                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: $"Warn snooper {a}",
                    valueName: @"warn snooper",
                    newVal: a);
            }
        }

        #endregion Warn Snooper
        #region Rimboard

        [Command("rimboard-enabled"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task RimboardEnabled(CommandContext ctx, bool enabled)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Program.UpdateSettings(ref Program.Settings.RimboardEnabled, enabled);

                string a = GetEnabledDisabled(enabled);
                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: $"Rimboard {a}",
                    valueName: @"warn snooper",
                    newVal: a);
            }
        }

        [Command("rimboard-channel"), Aliases("rimboard-chan"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task SetRimboardChannel(CommandContext ctx, DiscordChannel channel)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Program.UpdateSettings(ref Program.Settings.RimboardChannelId, channel.Id);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Rimboard channel changed",
                    valueName: @"Rimboard channel",
                    newVal: channel.Mention);
            }
        }

        [Command("rimboard-webhook"), MinimumRole(ConfigPerm), Hidden(), Description("WIP")]
        public async Task SetRimboardWebhook(CommandContext ctx, ulong id)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Program.UpdateSettings(ref Program.Settings.RimboardWebhookId, id);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Rimboard webhook id changed",
                    valueName: @"Rimboard webhook id",
                    newVal: id.ToString());
            }
        }

        [Command("rimboard-emote"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task SetRimboardEmote(CommandContext ctx, DiscordEmoji emote)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Program.UpdateSettings(ref Program.Settings.RimboardEmoticonId, emote.Id);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Rimboard emoji changed",
                    valueName: @"Rimboard emoji",
                    newVal: emote.ToString());
            }
        }

        [Command("rimboard-reaction"), Aliases("rimboard-reactions", "rimboard-reactions-required"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task SetRimboardReactionsRequired(CommandContext ctx, uint count)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                // Clamp it to the max value first so the bot doesn't crash.
                int countInt = (int)Math.Min(count, int.MaxValue);

                Program.UpdateSettings(ref Program.Settings.RimboardReactionsNeeded, countInt);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Rimboard reactions required changed",
                    valueName: @"Rimboard reaction requirement",
                    newVal: count.ToString());
            }
        }

        [Command("rimboard-pin-reaction"), Aliases("rimboard-pin-reactions", "rimboard-pin-reactions-required"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task SetRimboardReactionsRequiredToPin(CommandContext ctx, uint count)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                // Clamp it to the max value first so the bot doesn't crash.
                int countInt = (int)Math.Min(count, int.MaxValue);

                Program.UpdateSettings(ref Program.Settings.RimboardPinReactionsNeeded, countInt);

                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: @"Rimboard reactions to pin message required changed",
                    valueName: @"Rimboard reaction to pin requirement",
                    newVal: count.ToString());
            }
        }

        #endregion Rimboard
        #region Fun stuff

        [Command("fun-allowed"), MinimumRole(ConfigPerm), Description("WIP")]
        public async Task FunAllowed(CommandContext ctx, bool enabled)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                Program.UpdateSettings(ref Program.Settings.FunAllowed, enabled);

                string a = GetEnabledDisabled(enabled);
                await GenericResponses.SendMessageSettingChanged(
                    channel: ctx.Channel,
                    mention: ctx.Member.Mention,
                    title: $"Fun {a}",
                    valueName: @"fun allowed",
                    newVal: a);
            }
        }


        #endregion
    }
}
