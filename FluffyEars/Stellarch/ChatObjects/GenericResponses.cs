// GenericResponses.cs
// These are full generic, standardized responses for situations.
//
// EMIKO

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
                title: $"Invalid arguments: !{ctx.Command.Name}");

            await ctx.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        public static async Task SendMessageChangedNotChanged(DiscordChannel channel, 
                                                          string title, 
                                                          string mention, 
                                                          string body, 
                                                          Dictionary<DiscordChannel, bool> successChanged, 
                                                          string verb, 
                                                          string invertedVerb)
        {
            var deb = new DiscordEmbedBuilder(
                Generics.GenericEmbedTemplate(
                    color: Generics.NeutralColor,
                    description: Generics.NeutralDirectResponseTemplate(mention, body),
                    title: title));

            // ----------------
            // Changed:
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendJoin(' ', // Select all the values that were changed
                from a in successChanged
                where a.Value
                select a.Key);

            // Check if anything was added and then make a field if anything was.
            if(stringBuilder.Length > 0)
                deb.AddField(verb, value: stringBuilder.ToString());

            // ----------------
            // Not changed:
            stringBuilder = new StringBuilder();
            stringBuilder.AppendJoin(' ', // Select all the values that were not changed
                from a in successChanged
                where !a.Value
                select a.Key);

            // Check if anything was added and then make a field if anything was.
            if (stringBuilder.Length > 0)
                deb.AddField(invertedVerb, value: stringBuilder.ToString());

            // ----------------
            // Send something

            await channel.SendMessageAsync(embed: deb.Build());
        }

        public static async Task SendMessageSettingChanged(DiscordChannel channel,
                                                           string mention, 
                                                           string title,
                                                           string valueName,
                                                           string newVal)
        {
            var deb = new DiscordEmbedBuilder(
                Generics.GenericEmbedTemplate(
                    color: Generics.PositiveColor,
                    description: Generics.PositiveDirectResponseTemplate(mention, $"I changed the setting '{valueName}' to have the value '{newVal}'."),
                    title: title));

            await channel.SendMessageAsync(embed: deb.Build());
        }
    }
}
