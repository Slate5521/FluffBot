// BotSettings.cs
// A static class for every bot setting including their default values.
//
// EMIKO.

using System;
using System.Collections.Generic;
using System.Text;

namespace BigSister.Settings
{
    public class BotSettings
    {
        /// <summary>Default channel, FDS: #dont-ever-delete.</summary>
        /// I have it sent to a channel in my development server.
        const ulong DEFAULT_CHANNEL = 724187658158342204;

        public BotSettings() { }

        // ----------------
        // Filter

        /// <summary>Channels excluded from filtering.</summary>
        /// Not set to any default channels. Just want to initiate it so it isn't null.
        public List<ulong> ExcludedChannels = new List<ulong>();
        /// <summary>The filter channel.</summary>
        public ulong FilterChannelId = DEFAULT_CHANNEL;

        // ----------------
        // Reminder

        /// <summary>Maximum reminder time allowed in months.</summary>
        public int MaxTimeMonths = 12;

        // ----------------
        // Mention snooper

        /// <summary>ID of #action-logs</summary>
        public ulong ActionChannelId = DEFAULT_CHANNEL;
        /// <summary>Maximum timespan for an action to be considered.</summary>
        public int ActionTimespan = 6;
        /// <summary>If passive warn snooping is enabled.</summary>
        public bool AutoWarnSnoopEnabled = false;

        // ----------------
        // Rimboard

        /// <summary>ID of the Rimboard channel.</summary>
        public ulong RimboardChannelId = DEFAULT_CHANNEL;
        /// <summary>Webhook ID of the Rimboard channel.</summary>
        /// I have it set to a webhook in my development server.
        public ulong RimboardWebhookId = 752987486661247159;
        /// <summary>The Rimboard's pinning emote.</summary>
        public string RimboardEmoticon = @"<:rimworld:432227067313127424>";
        /// <summary>Number of reactions needed to pin a message in Rimboard.</summary>
        public int ReactionsNeeded = 5;

        // ----------------
        // Role Requests

        /// <summary>ID of the role request channel.</summary>
        public ulong RoleChannel = DEFAULT_CHANNEL;
    }
}
