// ReminderSystem.cs
// A portion of the reminder system containing everything needed for processing reminders from ReminderCommands.cs
//  
// EMIKO

using BigSister.Database;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace BigSister.Reminders
{
    public static partial class ReminderSystem
    {
        /// <summary>Query to add a reminder to the database.</summary>
        static string QQ_AddReminder = @"INSERT INTO `Reminders` (`Id`, `UserId`, `ChannelId`, `Message`, `TriggerTime`, `Mentions`) 
                                         VALUES ($id, $userid, $channelid, $message, $time, $mention);";
        /// <summary>Query to remove a reminder from the database.</summary>
        static string QQ_RemoveReminder = @"DELETE FROM `Reminders` WHERE `Id`=$id;";
        /// <summary>Query to check if a reminder exists.</summary>
        static string QQ_ReminderExists = @"SELECT EXISTS(SELECT 1 FROM `Reminders` WHERE `Id`=$id);";
        /// <summary>Query to read the entire reminder table.</summary>
        static string QQ_ReadTable = @"SELECT `UserId`, `ChannelId`, `Message`, `TriggerTime`, `Mentions` FROM `Reminders`;";
        /// <summary>Query to return all reminders that need to be triggered.</summary>
        static string QQ_CheckRemindersElapsed = @"SELECT `UserId`, `ChannelId`, `Message`, `TriggerTime`, `Mentions` 
                                                   FROM `Reminders` WHERE `TriggerTime` >= $timenow;";
        /// <summary>Query to delete all reminders that need to be triggered.</summary>
        static string QQ_DeleteRemindersElapsed = @"DELETE FROM `Reminders` WHERE `TriggerTime` >= $timenow;";


        #region ReminderCommands.cs

        /// <summary>The date recognition Regex.</summary>
        // It groups every number/time unit pairing "(number)+(time unit)" into a group. 
        static Regex DateRegex
            = new Regex(@"(\d+)\s?(months?|days?|d|weeks?|wks?|w|hours?|hrs?|h|minutes?|mins?)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        public static async Task AddReminder(CommandContext ctx, string args)
        {// Firstly get all the matches.
            MatchCollection regexMatches = DateRegex.Matches(args);
            BitArray regexCoverage = new BitArray(args.Length);
            var dto = ctx.Message.CreationTimestamp;
            List<ulong> mentions = new List<ulong>();

            // String processing - find the message and get the reminder end date. 
            // To find what's not a date, we simply look for the first character that isn't in the boundaries of a Regex match. 
            //
            // Structure of a possible string:
            //
            //             Date String | Message String
            //  DATE, DATE, DATE, DATE | message
            //
            // Beyond the Date String we want to stop processing time information as people may reference time in the message string, so we don't
            // erronously want that data added to the date string.


            // Populate the regexCoverage...
            foreach (Match match in regexMatches)
            {
                for (int i = match.Index; i < match.Index + match.Length; i++)
                {
                    regexCoverage[i] = true;
                }
            }

            // So I want to explain what I'm about to do here. Every value in regexCoverage[] indicates if that's part of the initial time
            // string. We want to use it to determine if something is a message or a time value, so if at any point, we run into something that
            // isn't a time string, we want to set every instance thereafter as false so we know it's part of a message.
            if (regexMatches.Count > 0)
            {
                bool value = regexCoverage[0];
                for (int k = 1; k < regexCoverage.Count; k++)
                {
                    if (!IsWhitespace(args[k]))
                    {
                        if (!regexCoverage[k] && value)
                        {
                            value = false;
                        }

                        if (!value)
                        {
                            regexCoverage[k] = value;
                        }
                    }
                }
            }
            // We need to figure out where the date string ends.
            string messageString = String.Empty;

            int dateEndIndex = 0;
            bool messageFound = false;
            while (dateEndIndex < regexCoverage.Length && !messageFound)
            {
                char stringChar = args[dateEndIndex];
                bool inRegexBoundaries = regexCoverage[dateEndIndex];

                // This checks to see if the character is non-white-space and outside of any RegEx boundaries.
                messageFound = !IsWhitespace(stringChar) && !inRegexBoundaries;

                // If not found, continue; otherwise, keep incrementing.
                if (!messageFound)
                {
                    dateEndIndex++;
                }
            }

            // If we aren't going out of bounds, let's set the string to this.
            if (dateEndIndex < regexCoverage.Length)
            {
                messageString = args.Substring(dateEndIndex);
            }

            // Get date information
            foreach (Match match in regexMatches)
            {
                // Only try to exclude Message String date information if a message string was found.
                if (!messageFound || (regexCoverage[match.Index] && regexCoverage[match.Index + match.Length - 1]))
                {
                    InterpretTime(match.Groups[1].Value, match.Groups[2].Value, ref dto);
                }
            }

            // Get mentions
            foreach (DiscordUser user in ctx.Message.MentionedUsers)
            {
                ulong id = user.Id;

                if (!user.IsBot && !mentions.Contains(id))
                {
                    mentions.Add(id);
                }
            }

            // At this point, now we have the DateTimeOffset describing when this reminder needs to be set off, and we have a message string if
            // any. So now we just need to make sure it's within reasonable boundaries, set the reminder, and notify the user.

            DateTimeOffset maxtime = new DateTimeOffset(ctx.Message.CreationTimestamp.UtcDateTime).AddMonths(Program.Settings.MaxTimeMonths);
            DiscordEmbed embed;

            if (dto.UtcTicks == ctx.Message.CreationTimestamp.UtcTicks)
            {   // No time was added.
                /*embed = ChatObjects.FormatEmbedResponse
                    (
                        title: @"Unable to Add Reminder",
                        description: ChatObjects.GetErrMessage(@"I was unable able to add the mask you gave me. You didn't supply me a valid time..."),
                        color: ChatObjects.ErrColor,
                        thumbnail: ChatObjects.URL_REMINDER_GENERIC
                    );*/
            }
            else if (dto.UtcTicks > maxtime.UtcTicks)
            {   // More than a year away.
                /*embed = ChatObjects.FormatEmbedResponse
                    (
                        title: @"Unable Add Reminder",
                        description: ChatObjects.GetErrMessage(@"I was unable able to add the mask you gave me. That's more than a year away..."),
                        color: ChatObjects.ErrColor,
                        thumbnail: ChatObjects.URL_REMINDER_GENERIC
                    );*/
            }
            else
            {   // Everything is good in the world... except that the world is burning, but that's not something we're worried about here, for
                // now...
                /*embed = ChatObjects.FormatEmbedResponse
                    (
                        title: @"Add Reminder",
                        description: ChatObjects.GetSuccessMessage(@"I added the reminder you gave me!"),
                        color: ChatObjects.SuccessColor,
                        thumbnail: ChatObjects.URL_REMINDER_GENERIC
                    );*/

                Reminder reminder = new Reminder
                {
                    Text = messageString.Length.Equals(0) ? @"n/a" : messageString.ToString(),
                    Time = (int)(dto.ToUnixTimeSeconds() / 60),
                    User = ctx.Member.Id,
                    Channel = ctx.Channel.Id,
                    UsersToNotify = mentions.Select(a => ChatObjects.Generics.GetMention(a)).ToArray()
                };

                // Let's build the command.
                using var command = new SqliteCommand(BotDatabase.Instance.DataSource);
                command.CommandText = QQ_AddReminder;

                SqliteParameter a = new SqliteParameter("$id", reminder.GetIdentifier());
                a.DbType = DbType.String;

                SqliteParameter b = new SqliteParameter("$userid", reminder.User);
                b.DbType = DbType.String;

                SqliteParameter c = new SqliteParameter("$channelid", reminder.Channel);
                c.DbType = DbType.String;

                SqliteParameter d = new SqliteParameter("$message", reminder.Text);
                d.DbType = DbType.String;

                SqliteParameter e = new SqliteParameter("$time", reminder.Time);
                e.DbType = DbType.Int64;

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendJoin(' ', reminder.UsersToNotify);

                SqliteParameter f = new SqliteParameter("$mention", stringBuilder.ToString());
                f.DbType = DbType.String;

                command.Parameters.AddRange(new SqliteParameter[] { a, b, c, d, e, f });

                await BotDatabase.Instance.ExecuteNonQuery(command);
            }
        }

        /// <summary>Remove a reminder if it exists.</summary>
        public static async Task RemoveReminder(CommandContext ctx, string args)
        {
            // It's a reminder, so let's remove it.

            // Let's build the command.
            using var command = new SqliteCommand(BotDatabase.Instance.DataSource);
            command.CommandText = QQ_RemoveReminder;

            SqliteParameter a = new SqliteParameter("$id", args);
            a.DbType = DbType.String;

            command.Parameters.Add(a);

            await BotDatabase.Instance.ExecuteNonQuery(command);
        }

        /// <summary>Check if a provided ID is a reminder.</summary>
        public static async Task<bool> IsReminder(string id)
        {
            bool hasItem_returnVal;

            // Let's build the command.
            using var command = new SqliteCommand(BotDatabase.Instance.DataSource);
            command.CommandText = QQ_ReminderExists;

            SqliteParameter a = new SqliteParameter("$id", id);
            a.DbType = DbType.String;

            command.Parameters.Add(a);

            object returnVal = await BotDatabase.Instance.ExecuteReaderAsync(command,
                    processAction: delegate (SqliteDataReader reader)
                    {
                        object a;

                        if (reader.Read())
                        {   // Let's read the database.
                            a = reader.GetValue(0);
                        }
                        else
                        {
                            a = null;
                        }

                        return a;
                    });

            int returnValC;

            // Try to convert it to an int. If it throws an exception for some reason, chances are it's not what we're looking for.
            try
            {
                returnValC = Convert.ToInt32(returnVal);
            }
            catch
            {   // Probably not an int, so let's set the value to something we absolutely know will return as false.
                returnValC = -1;
            }

            // Let's get the return value by checking if the returnval == 1
            hasItem_returnVal = returnValC == 1;

            return hasItem_returnVal;
        }

        /// <summary>List all the reminders</summary>
        public static async Task ListReminders(CommandContext ctx)
        {
            Reminder[] reminders = await ReadTable();

            StringBuilder sb = new StringBuilder();
            foreach(Reminder reminder in reminders)
            {
                sb.Append($"{reminder.GetIdentifier()}|{reminder.User}|{reminder.Text}|{reminder.Time}|");
                sb.AppendJoin(' ', reminder.UsersToNotify);
                sb.Append('\n');
            }

            await ctx.Channel.SendMessageAsync("fuck kks\n" + sb.ToString());
        }

        static Func<SqliteDataReader, object> readReminders = 
            delegate (SqliteDataReader reader)
                {
                    var reminderList = new List<Reminder>();

                    while (reader.Read())
                    {   // Generate a reminder per each row.

                        var r = new Reminder
                        {
                            User    = ulong.Parse(reader.GetString(0)),
                            Channel = ulong.Parse(reader.GetString(1)),
                            Text    = reader.GetString(2),
                            Time    = reader.GetInt32(3),
                            UsersToNotify = reader.GetString(4).Split(' ')
                        };

                        reminderList.Add(r);
                    }

                    return reminderList.ToArray();
                };


        /// <summary>Read the table and return as an array.</summary>
        public static async Task<Reminder[]> ReadTable()
        {
            using var command = new SqliteCommand(BotDatabase.Instance.DataSource);
            command.CommandText = QQ_ReadTable;

            Reminder[] returnVal = (Reminder[])await BotDatabase.Instance.ExecuteReaderAsync(command,
                processAction: readReminders);

            return returnVal;
        }

        /// <summary>Find any reminders that need to be triggered and trigger them.</summary>
        static async Task LookTriggerReminders(int timeNowMinutes)
        {
            using var command = new SqliteCommand(BotDatabase.Instance.DataSource);
            command.CommandText = QQ_CheckRemindersElapsed;

            SqliteParameter a = new SqliteParameter("$timenow", timeNowMinutes);
            a.DbType = DbType.Int32;

            command.Parameters.Add(a);

            Reminder[] pendingReminders = (Reminder[])await BotDatabase.Instance.ExecuteReaderAsync(command,
                processAction: readReminders);

            // Check if there are any reminders
            if (pendingReminders.Length > 0)
            {   // There are reminders.
                using var delCommand = new SqliteCommand(BotDatabase.Instance.DataSource);
                delCommand.CommandText = QQ_DeleteRemindersElapsed;

                delCommand.Parameters.Add(a);

                await BotDatabase.Instance.ExecuteNonQuery(command);
            }
        }

        #endregion ReminderCommands.cs

        /// <summary>Interpret time value and increment a DateTimeOffset based on the values.</summary>
        /// <param name="measureString">The measure or numeric value.</param>
        /// <param name="unit">The time unit.</param>
        /// <param name="dto">The DateTimeOffset to increment.</param>
        private static void InterpretTime(string measureString, string unit, ref DateTimeOffset dto)
        {
            // Only continue if these two have a valid value.
            if (int.TryParse(measureString, out int measure) && measure > 0 && unit.Length > 0)
            {
                switch (unit.ToLower())
                {
                    case "month":
                    case "months":
                        dto = dto.AddMonths(measure);
                        break;
                    case "day":
                    case "days":
                    case "d":
                        dto = dto.AddDays(measure);
                        break;
                    case "week":
                    case "weeks":
                    case "wk":
                    case "wks":
                    case "w":
                        dto = dto.AddDays(measure * 7);
                        break;
                    case "hour":
                    case "hours":
                    case "hr":
                    case "hrs":
                    case "h":
                        dto = dto.AddHours(measure);
                        break;
                    case "minute":
                    case "minutes":
                    case "min":
                    case "mins":
                        dto = dto.AddMinutes(measure);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>Checks if a character is a white space.</summary>
        private static bool IsWhitespace(char c)
        {
            return c.Equals(' ')  ||
                   c.Equals('\r') ||
                   c.Equals('\n') ||
                   c.Equals('\t');
        }

        internal static async void ReminderTimer_Elapsed(object sender, ElapsedEventArgs e)
            => await LookTriggerReminders(
                (int)(e.SignalTime.ToUniversalTime().Ticks / (TimeSpan.TicksPerMillisecond * 1000 * 60)));
    }
}
