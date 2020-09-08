// BotDatabase.cs
// Contains methods for accessing the SQLite database. So far there are four tables:
//  1) Rimboard =================================================================  ________________________________________________
//  |   OriginalMessageId               |   PinnedMessageId                     |  | __|  \/  |_ _| |/ / _ \    THE
//  |-----------------------------------+---------------------------------------|  | _|| |\/| || || ' < (_) |       BEST
//  |   String representing UInt64      |   String representing UInt64          |  |___|_|  |_|___|_|\_\___/            SNEPBUN
//  |   Snowflake of original message   |   Snowflake of pinned aka reposted    |                                           AROUND
//  |                                   |       message.                        |
//  2) Reminders    =============================================================================================================================================================================================================================
//  |   Id                              |   UserId                              |   ChannelId                           |   Message                             |   TriggerTime                         |   Mentions                            |
//  |-----------------------------------+---------------------------------------+---------------------------------------+---------------------------------------+---------------------------------------+---------------------------------------|
//  |   TinyText not Null               |   String representing UInt64 not Null |   String representing UInt64 not Null |   MediumText                          |   BigInt not Null                     |   MediumText                          |
//  |   Reminder ID (not the message    |   The user who created the reminder.  |   Snowflake of the channel the        |   Reminder message                    |   Reminder trigger timestamp in Unix  |   Whitespace separated mentions       |
//  |       snowflake)                  |                                       |   reminder was sent in                |                                       |   epoch UTC                           |                                       |
//  3) Filter   =================================================================================================================================================================================================================================
//  |   Type                            |   String                              |
//  |-----------------------------------+---------------------------------------|(\(\               
//  |   TinyInt not Null Default '1'    |   MediumText not Null                 |(-,-)      <-- bunnies ftw
//  |   1 = Regex Mask and 2 = Exclude  |   Mask of Regex or Exclude            |o_(")(")
//  4) Roles    =========================================================================================================
//  |   MessageId                       |   RoleId                              |   EmoteId                             |
//  |-----------------------------------+---------------------------------------+---------------------------------------|
//  |String representing UInt64 not Null| String representing UInt64 not Null   |   String representing UInt64 not Null |
//  |   Snowflake of message            |   Snowflake of role                   |   Snowflake of emote                  |
//  =====================================================================================================================
//
// EMIKO

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BigSister.Database
{
    public sealed class BotDatabase
    {
        private static BotDatabase instance =
            new BotDatabase(Program.Files.DatabaseFile);

        SemaphoreSlim semaphoreSlim;

        /// <summary>Database connection datasource.</summary>
        public string DataSource;

        public static BotDatabase Instance
        {
            get => instance;
        }

        static BotDatabase() { }

        BotDatabase(string uri)
        {
            DataSource = $"Data Source={uri}";
            semaphoreSlim = new SemaphoreSlim(1, 1);
        }

        ~BotDatabase()
        {
            semaphoreSlim.Dispose();
        }

        public async Task<object> ExecuteScalarAsync(SqliteCommand cmd, Func<SqliteDataReader, object> processAction)
        {
            object returnVal;

            semaphoreSlim.Wait();

            try
            {
                using(var connection = new SqliteConnection(DataSource))
                {
                    DataSet ds = new DataSet();

                    cmd.Connection = connection;

                    connection.Open();

                    using (SqliteDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        returnVal = processAction.Invoke(reader);
                    }                    

                    connection.Close();
                    connection.Dispose();
                }
            } 
            finally
            {
                semaphoreSlim.Release();
            }

            return returnVal;
        }

        public async Task ExecuteNonQuery(SqliteCommand cmd)
        {
            semaphoreSlim.Wait();

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    cmd.Connection = connection;

                    connection.Open();

                    await cmd.ExecuteNonQueryAsync();

                    connection.Close();
                    connection.Dispose();
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        /// <summary>Generate a default database file.</summary>
        public static void GenerateDefaultFile(string outputFile)
        {   //Data Source=c:\mydb.db;Version=3;
            string dataSource = $"Data Source={outputFile};";

            using (var connection = new SqliteConnection(dataSource))
            {
                connection.Open();

                using var command = connection.CreateCommand();

                // --------------------------------
                // Rimboard table.

                command.CommandText =
                    @"
                        CREATE TABLE `Rimboard` (
                            `OriginalMessageId` TEXT NOT NULL, -- Snowflake of original message.
                            `PinnedMessageId`   TEXT NOT NULL  -- Snowflake of pinned aka reposted message.
                        );
                    ";

                command.ExecuteNonQuery();

                // --------------------------------
                // Reminder table.

                command.CommandText =
                    @"
                        CREATE TABLE `Reminders` (
	                        `Id`            TEXT    NOT NULL, -- Reminder ID
	                        `UserId`        TEXT    NOT NULL, -- Snowflake of user who created the reminder
	                        `ChannelId`     TEXT    NOT NULL, -- Snowflake of channel the reminder was created in
	                        `Message`       TEXT            , -- Reminder message
	                        `TriggerTime`   INTEGER NOT NULL, -- Reminder trigger timestamp
	                        `Mentions`      TEXT              -- Whitespace separated mention strings
                        );
                    ";

                command.ExecuteNonQuery();

                // --------------------------------
                // Exclude system.

                command.CommandText =
                    @"
                        CREATE TABLE `Filter` (
                            `Type`      INTEGER unsigned NOT NULL DEFAULT '1', -- 1 REGEX 2 EXCLUDE
                            `String`    TEXT             NOT NULL              -- Mask of regex or exclude
                        );
                    ";

                command.ExecuteNonQuery();

                // --------------------------------
                // Role request.

                command.CommandText =
                    @"
                        CREATE TABLE `Roles` (
	                        `MessageId` TEXT NOT NULL, -- Snowflake of message
	                        `RoleId`    TEXT NOT NULL, -- Snowflake of role
	                        `EmoteId`   TEXT NOT NULL  -- Snowflake of emote
                        );
                    ";
            }
        }
    }
}
