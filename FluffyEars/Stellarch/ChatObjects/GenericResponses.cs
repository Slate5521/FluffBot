// GenericResponses.cs
// These are full generic, standardized responses for situations.
//
// EMIKO

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BigSister.ChatObjects
{
    public static class GenericResponses
    {
        public static DiscordEmbed GetMessageInsufficientPermissions(string mention,
                                                                      string minRole, 
                                                                      string command)
        {
            string description = Generics.NegativeDirectResponseTemplate(mention,
                body: $"you do not have the permissions to use the command '{Formatter.Bold(command)}'. Required role: {Formatter.Bold(minRole)}...");

            return Generics.GenericEmbedTemplate(Generics.NegativeColor, description, title: @"Insufficient permission");


        }

        public static async Task HandleInvalidArguments(CommandContext ctx) 
        {
            string description = Generics.NegativeDirectResponseTemplate(ctx.Member.Mention,
                body: $"I did not understand the arguments supplied to me. Please check that you have the right arguments...\n\n" +
                      $"**Description:** {ctx.Command.Description}");

            var embedBuilder = Generics.GenericEmbedTemplate(Generics.NegativeColor, description,
                title: $"Invalid arguments: {ctx.Command.Name}");

            await ctx.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
