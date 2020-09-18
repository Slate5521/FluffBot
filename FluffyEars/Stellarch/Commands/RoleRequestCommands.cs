﻿// RoleRequestCommands.cs
// Contains commands for adding/removing embeds plus commands for adding messages:
//  !role-embed-create <channel> <emoji> <role> <title> - Creates a new embed with a title and initial emoji and role.
//  !role-embed-append <channel> <messageId> <emoji> <role> - Appends a new emoji onto an existing embed.
//  !role-embed-remove <channel> <messageId> <emoji> - Removes a role from an embed.
//  !role-message-new <channel> <content> - Creates a new message in a channel.
//  !role-message-edit <channel> <messageId> <content> - Edits a currently existing message.
//
// EMIKO

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using BigSister.RoleRequest;
using BigSister.ChatObjects;

namespace BigSister.Commands
{
    [Group("Roles")]
    class RoleRequestCommands : BaseCommandModule
    {
        [Command("role-embed-create"),
            MinimumRole(Role.BotManager),
            Description("Adds a role embed in the specified channel with the initial role, emoji, and title.\n\n**Usage:**\n" +
               "!role-embed-create <#channel> <emoji> <role> <title>")]
        public async Task RoleEmbedCreate(CommandContext ctx, DiscordChannel channel, DiscordEmoji emoji, DiscordRole role, [RemainingText] string title)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {

                if (!(title is null) && title.Length > 0)
                {
                    await RoleRequestSystem.RoleEmbedCreate(ctx, channel, title, emoji, role);
                }
                else
                {
                    await GenericResponses.HandleInvalidArguments(ctx);
                }
            }
        }

        [Command("role-embed-append"),
            MinimumRole(Role.BotManager),
            Description("Appends an emoji/role pair to an existing embed in the specified channel.\n\n**Usage:**\n" +
               "!role-embed-append <#channel> <messageId> <emoji> <role>")]
        public async Task RoleEmbedAppendRole(CommandContext ctx, DiscordChannel channel, ulong messageId, DiscordEmoji emoji, DiscordRole role)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleEmbedAppendRole(ctx, channel, messageId, emoji, role);
            }
        }

        [Command("role-embed-remove"),
            MinimumRole(Role.BotManager),
            Description("Removes an emoji/role pair from an existing embed in the specified channel.\n\n**Usage:**\n" +
               "!role-embed-remove <#channel> <emoji>")]
        public async Task RoleEmbedRemoveRole(CommandContext ctx, DiscordChannel channel, ulong messageId, DiscordEmoji emoji)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleEmbedRemoveRole(ctx, channel, messageId, emoji);
            }
        }

        [Command("role-message-new"),
            MinimumRole(Role.BotManager),
            Description("Creates a new blank message unassociated with database information in the specified channel.\n\n**Usage:**\n" +
               "!role-message-new <#channel> <content>")]
        public async Task RoleMessageNew(CommandContext ctx, DiscordChannel channel, [RemainingText] string content)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleMessageNew(channel, content);
            }
        }

        [Command("role-message-edit"),
            MinimumRole(Role.BotManager),
            Description("Edits a message in the specified channel.\n\n**Usage:**\n" +
               "!role-message-edit <#channel> <messageId> <content>")]
        public async Task RoleMessageEdit(CommandContext ctx, DiscordChannel channel, ulong messageId, [RemainingText] string content)
        {
            if (await Permissions.HandlePermissionsCheck(ctx))
            {
                await RoleRequestSystem.RoleMessageEdit(channel, messageId, content);
            }
        }
    }
}
