using BigSister.ChatObjects;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BigSister.Commands
{
    public class BunnyHelpFormatter : BaseHelpFormatter
    {
        readonly bool userHasPerms;
        readonly DiscordEmbedBuilder embedBuilder;

        bool isHelp = true;

        public BunnyHelpFormatter(CommandContext ctx) : base(ctx)
        {
            userHasPerms =
                Permissions.HandlePermissionsCheck(
                    member: ctx.Member,
                    chan: ctx.Channel,
                    new MinimumRole(Role.CS),
                    shouldRespondToRejection: false).ConfigureAwait(false).GetAwaiter().GetResult();

            embedBuilder = new DiscordEmbedBuilder()
            {
                Color = Generics.NeutralColor
            };
        }

        public override CommandHelpMessage Build()
        {
            CommandHelpMessage @return;

            if(userHasPerms)
            {
                @return = new CommandHelpMessage(embed: embedBuilder.Build());
            }
            else
            {
                @return = new CommandHelpMessage(@"You cannot use that.");
            }

            return @return;
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            isHelp = false;

            if (userHasPerms)
            {
                embedBuilder.WithTitle($"Help - {command.Name}");

                string description = command.Description.Length > 0 ?
                                     command.Description :
                                     @"No description provided.";

                embedBuilder.WithDescription($"`{command.Name}`: {description}");

                // Check if this is a command group.
                if (!IsCommandGroup(command))
                {   // It's a command.

                    embedBuilder.WithTitle(@"Help");

                    embedBuilder.WithDescription(
                        $"{Generics.NEUTRAL_PREFIX} {description}");

                    // Get all of its aliases.
                    if (command.Aliases.Count > 0)
                    {
                        StringBuilder a = new StringBuilder();

                        foreach (string alias in command.Aliases)
                        {
                            a.Append($"`{alias}` ");
                        }

                        a.Remove(a.Length - 1, 1);
                        a.Replace(@" ", @", ");

                        embedBuilder.AddField(@"Aliases", a.ToString());
                    }

                    if (command.Overloads.Count() > 0)
                    {
                        StringBuilder a = new StringBuilder();

                        for(int i = 0; i < command.Overloads.Count(); i++) 
                        {
                            CommandOverload overload = command.Overloads[i];

                            for(int j = 0; j < overload.Arguments.Count; j++) 
                            {
                                CommandArgument arg = overload.Arguments[j];
                                bool last = j + 1 == overload.Arguments.Count;

                                // `Name (type) default
                                a.Append($"`{arg.Name} ({arg.Type.Name})");
                                if (!(arg.DefaultValue is null))
                                {
                                    a.Append($"default = {arg.DefaultValue}");
                                }

                                a.Append("`: ");

                                if (arg.IsOptional)
                                {
                                    a.Append(@"(Optional)");
                                }

                                a.Append(' ');

                                if(arg.Description.Length > 0)
                                {
                                    a.Append(arg.Description);
                                }
                                else
                                {
                                    a.Append(@"No description provided.");
                                } 

                                if(!last)
                                {
                                    a.Append('\n');
                                }

                            } // end foreach

                            string fieldTitle = i == 0 ? @"Arguments" : $"Arguments Overload {i + 1}";

                            embedBuilder.AddField(fieldTitle, a.ToString());
                        } // end for
                    } // end if
                } // end if
                else
                {   // It's a command group.
                    embedBuilder.WithTitle($"Help - {command.Name}");

                    embedBuilder.WithDescription(
                        $"{Generics.NEUTRAL_PREFIX} `{command.Name}`: {description}");
                } // end else
            } // end if

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if(userHasPerms)
            {
                var subcommandsSb = new StringBuilder();
                var subgroupsSb = new StringBuilder();
                int count = subcommands.Count();
                bool subcommandsFound = false;
                bool subgroupsFound = false;

                if (isHelp)
                {
                    embedBuilder.WithDescription(
                        $"{Generics.NEUTRAL_PREFIX} Listing all top-level commands and groups. Specify a command or group to see more information.");
                }

                for (int i = 0; i < count; i++)
                {
                    Command cmd = subcommands.ElementAt(i);

                    // I don't want !help in the listing. It's pointless.
                    if (!cmd.Name.Equals(@"help"))
                    {                     
                        // Check if it's a command group type.
                        if (IsCommandGroup(cmd))
                        {   // It's a command group, so we want to list it as such.
                            subgroupsSb.Append($"`{cmd.Name}` ");

                            if (!subgroupsFound)
                                subgroupsFound = true;
                        }
                        else
                        {   // It's a regular command, so let's list it as such.
                            subcommandsSb.Append($"`{cmd.Name}` ");

                            if (!subcommandsFound)
                                subcommandsFound = true;
                        }
                    }
                }

                if (subcommandsFound)
                {
                    subcommandsSb.Remove(subcommandsSb.Length - 1, 1);
                    subcommandsSb.Replace(@" ", @", ");

                    embedBuilder.AddField(@"Commands", subcommandsSb.ToString());
                }

                if(subgroupsFound)
                {
                    subgroupsSb.Remove(subgroupsSb.Length - 1, 1);
                    subgroupsSb.Replace(@" ", @", ");

                    embedBuilder.AddField(@"Groups", subgroupsSb.ToString());
                }
            }

            return this;
        }

        protected static bool IsCommandGroup(Command cmd)
            => cmd.GetType().Equals(typeof(CommandGroup));

        protected static string GetPermString(Command cmd)
        {
            var linq = cmd.CustomAttributes.OfType<MinimumRole>();

            return (linq.Count() > 0) switch
            {
                true    => UserPermissions.GetRoleString(linq.First().MinRole),
                false   => String.Empty
            };
        }
    }
}
