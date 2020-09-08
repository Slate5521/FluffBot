// FilterSystem.Commands.cs
// A portion of the filter system containing everything needed for processing commands from FilterCommands.cs
// 1) Unwraps commands coming from FilterCommands.cs and responds to those commands:
// 2) Contains methods for querying the database for relevant information.
// 3) Updating the regex array whenever the cache updates.
//
// EMIKO

using BigSister.Database;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using System.Text.RegularExpressions;
using DSharpPlus.EventArgs;

namespace BigSister.Filter
{
    public static partial class FilterSystem
    {
        // This region handles everything coming from FilterCommands.cs
        #region FilterCommands.cs

        /// <summary>Mask data type.</summary>
        public const int TYPE_MASK = 1;
        /// <summary>Exclude data type.</summary>
        public const int TYPE_EXCLUDE = 2;

        /// <summary>Query to check if a mask or exclude exists in the database.</summary>
        static string QQ_ItemExists = @"SELECT EXISTS(SELECT 1 FROM `FILTER` WHERE `STRING`=$string);";
        /// <summary>Query to add a mask or exclude into the database.</summary>
        static string QQ_ItemAdd = @"INSERT INTO `Filter` (`Type`, `String`) VALUES ($type, $string);";
        /// <summary>Query to remove a mask or exclude into the database.</summary>
        static string QQ_ItemRemove = @"DELETE FROM `Filter` WHERE `Type`=$type AND `String`=$string;";
        /// <summary>Query to read the table for either a mask or an exclude.</summary>
        static string QQ_ReadTable = @"SELECT `String` FROM `Filter` WHERE `Type`=$type;";

        static RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;

        static string[] MaskCache;
        static string[] ExcludeCache;
        static Regex[] FilterRegex;

        static FilterSystem() { }

        /// <summary>
        /// please call me when program starts
        /// </summary>
        public static void Initialize()
        {
            // bun makes cache

            UpdateCache()
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>Check if a mask exists in the database.</summary>
        public static async Task<bool> HasMask(string mask)
            => await HasItem(TYPE_MASK, mask);

        /// <summary>Check if an exclude exists in the database.</summary>
        public static async Task<bool> HasExclude(string exclude)
            => await HasItem(TYPE_EXCLUDE, exclude);
        
        /// <summary>Check if an item (possibly a mask or an exclude) exists in the database.</summary>
        static async Task<bool> HasItem(int type, string item)
        {
            bool hasItem_returnVal;

            // Check the cache.
            if (ExistsInCache(type, item))
            {   // In cahce
                hasItem_returnVal = true;
            }
            else
            { // Not in caches o we have ot lolok for it

                // Let's build the command.
                var command = new SqliteCommand(BotDatabase.Instance.DataSource);
                command.CommandText = QQ_ItemExists;

                SqliteParameter a = new SqliteParameter("$type", type);
                a.DbType = DbType.Byte;

                SqliteParameter b = new SqliteParameter("$string", item);
                b.DbType = DbType.String;

                command.Parameters.Add(a);
                command.Parameters.Add(b);

                object returnVal = await BotDatabase.Instance.ExecuteScalarAsync(command,
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
            }

            return hasItem_returnVal;
        }

        /// <summary>Add a mask to the database.</summary>
        public static async Task AddMask(CommandContext ctx, string mask)
            => await AddItem(ctx, TYPE_MASK, mask);

        /// <summary>Adds an exclude to the database.</summary>
        public static async Task AddExclude(CommandContext ctx, string exclude)
            => await AddItem(ctx, TYPE_EXCLUDE, exclude);

        /// <summary>Try to add an item to the database.</summary>
        static async Task AddItem(CommandContext ctx, int type, string item)
        {
            // Check the cache.
            if (ExistsInCache(type, item))
            {   // It's in the cache, so we need to let th e use know th at they it's in the cache
                await ctx.Channel.SendMessageAsync("thats already a thing inthere");
            }
            else
            {   // It's not in the cache so we can add it to the thing
                // Let's build the command.
                var command = new SqliteCommand(BotDatabase.Instance.DataSource);
                command.CommandText = QQ_ItemAdd;

                var a = new SqliteParameter("$type", type);
                a.DbType = DbType.Byte;

                var b = new SqliteParameter("$string", item);
                b.DbType = DbType.String;

                command.Parameters.Add(a);
                command.Parameters.Add(b);

                // Add it to the database.
                await BotDatabase.Instance.ExecuteNonQuery(command);

                await ctx.Channel.SendMessageAsync("hey man I just added that item thanks m");
                await UpdateCache();
            }
        }

        /// <summary>Remove a mask from the database.</summary>
        public static async Task RemoveMask(CommandContext ctx, string mask)
            => await RemoveItem(ctx, TYPE_MASK, mask);
        /// <summary>Remove an exclude from the database.</summary>
        public static async Task RemoveExclude(CommandContext ctx, string exclude)
            => await RemoveItem(ctx, TYPE_EXCLUDE, exclude);
        /// <summary>Remove an item from the database.</summary>
        static async Task RemoveItem(CommandContext ctx, int type, string item)
        {
            // Check if it's in cache.
            if (!ExistsInCache(type, item))
            {   // It's in the cache, so we need to let th e use know th at they it's in the cache
                await ctx.Channel.SendMessageAsync("thats not already a thing inthere");
            }
            else
            {
                // Build commdand

                var command = new SqliteCommand(BotDatabase.Instance.DataSource);
                command.CommandText = QQ_ItemRemove;

                var a = new SqliteParameter("$type", type);
                a.DbType = DbType.Byte;

                var b = new SqliteParameter("$string", item);
                b.DbType = DbType.String;

                command.Parameters.Add(a);
                command.Parameters.Add(b);

                // please remove it from the database thank you
                await BotDatabase.Instance.ExecuteNonQuery(command);

                await ctx.Channel.SendMessageAsync("hey man I just re,ovedd that item thanks m");
                await UpdateCache();
            }
        }

        /// <summary>List all the masks in the database.</summary>
        public static async Task ListMasks(CommandContext ctx)
            => await ListItems(ctx, TYPE_MASK);
        /// <summary>List all the excludes in the database.</summary>
        public static async Task ListExcludes(CommandContext ctx)
            => await ListItems(ctx, TYPE_EXCLUDE);
        /// <summary>List all the items in the database.</summary>
        public static async Task ListItems(CommandContext ctx, int type)
        {
            string[] shit = await ReadTable(type);

            StringBuilder sb = new StringBuilder();
            sb.AppendJoin(' ', shit);

            await ctx.Channel.SendMessageAsync("fuck kks\n" + sb.ToString());
        }
        /// <summary>Read a column from the filter table and return it as an array.</summary>
        public static async Task<string[]> ReadTable(int type)
        {
            var command = new SqliteCommand(BotDatabase.Instance.DataSource);
            command.CommandText = QQ_ReadTable;

            var a = new SqliteParameter("$type", type);
            a.DbType = DbType.Byte;

            command.Parameters.Add(a);

            string[] returnVal = (string[])await BotDatabase.Instance.ExecuteScalarAsync(command,
                processAction: delegate (SqliteDataReader reader)
                {
                    var rows = new List<string>();

                    while (reader.Read())
                    {   // From each row in column String add the result to the list.
                        rows.Add(reader.GetString(0));
                    }

                    return rows.ToArray();
                });

            return returnVal;
        }

        /// <summary>Checks if an item is in cache.</summary>
        static bool ExistsInCache(int type, string searchItem)
        {
            bool exists_returnVal; //r eturn value

            switch(type)
            {
                case TYPE_MASK:
                    exists_returnVal = MaskCache.Contains(searchItem);
                    break;
                case TYPE_EXCLUDE:
                    exists_returnVal = ExcludeCache.Contains(searchItem);
                    break;
                default: // why are you here?
                    exists_returnVal = false;
                    break;
            }

            return exists_returnVal;
        }

        /// <summary>Update the cache.</summary>
        static async Task UpdateCache()
        {
            MaskCache = await ReadTable(TYPE_MASK);
            ExcludeCache = await ReadTable(TYPE_EXCLUDE);

            // Update the regex array.
            List<string> maskListDesc = MaskCache.ToList();

            maskListDesc.Sort();
            maskListDesc.Reverse();

            // Initiate a regex value for every mask.
            FilterRegex = new Regex[maskListDesc.Count];
            for (int i = 0; i < maskListDesc.Count; i++)
            {
                FilterRegex[i] = new Regex(maskListDesc[i], regexOptions);
            }
        }


        #endregion FilterCommands.cs
    }
}
