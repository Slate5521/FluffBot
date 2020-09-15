using BigSister.ChatObjects;
using BigSister.RoleRequest;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BigSister.Commands
{
    class RoleRequestCommands : BaseCommandModule
    {
        [Command("role-embed-create"),
            MinimumRole(Role.BotManager),
            Description("WIP")]
        public async Task RoleEmbedCreate(CommandContext ctx, DiscordChannel channel, DiscordEmoji emoji, DiscordRole role, [RemainingText] string title)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleEmbedCreate(ctx, channel, title, emoji, role);
            }
        }

        [Command("role-embed-append"),
            MinimumRole(Role.BotManager),
            Description("WIP")]
        public async Task RoleEmbedAppendRole(CommandContext ctx, DiscordChannel channel, ulong messageId, DiscordEmoji emoji, DiscordRole role)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleEmbedAppendRole(ctx, channel, messageId, emoji, role);
            }
        }

        [Command("role-embed-remove"),
            MinimumRole(Role.BotManager),
            Description("WIP")]
        public async Task RoleEmbedRemoveRole(CommandContext ctx, DiscordChannel channel, ulong messageId, DiscordEmoji emoji)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleEmbedRemoveRole(ctx, channel, messageId, emoji);
            }
        }

        [Command("role-message-new"),
            MinimumRole(Role.BotManager),
            Description("WIP")]
        public async Task RoleMessageNew(CommandContext ctx, DiscordChannel channel, [RemainingText] string content)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleMessageNew(ctx, channel, content);
            }
        }

        [Command("role-message-edit"),
            MinimumRole(Role.BotManager),
            Description("WIP")]
        public async Task RoleMessageEdit(CommandContext ctx, DiscordChannel channel, ulong messageId, [RemainingText] string content)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleMessageEdit(ctx, channel, messageId, content);
            }
        }
    }
}
