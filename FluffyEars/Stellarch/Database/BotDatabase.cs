// BotDatabase.cs
// Contains methods for accessing the SQLite database. So far there are four tables:
//  1) Rimboard =================================================================  ________________________________________________
//  |   OriginalMessageId               |   PinnedMessageId                     |  | __|  \/  |_ _| |/ / _ \    THE
//  |-----------------------------------+---------------------------------------|  | _|| |\/| || || ' < (_) |       BEST
//  |   Unsigned BigInt not Null        |   Unsigned BigInt not Null            |  |___|_|  |_|___|_|\_\___/            SNEPBUN
//  |   Snowflake of original message   |   Snowflake of pinned aka reposted    |                                           AROUND
//  |                                   |       message.                        |
//  2) Reminders    =============================================================================================================================================
//  |   Id                              |   Message                             |   TriggerTime                         |   Mentions                            |
//  |-----------------------------------+---------------------------------------+---------------------------------------+---------------------------------------|
//  |   TinyText not Null               |   MediumText                          |   TimeStamp not Null                  |   MediumText not Null                 |
//  |   Reminder ID (not the message    |   Reminder message                    |   Reminder trigger timestamp          |   Whitespace separated mentions       |
//  |       snowflake)                  |                                       |                                       |                                       |
//  3) Filter   =================================================================================================================================================
//  |   Type                            |   String                              |
//  |-----------------------------------+---------------------------------------|(\(\               
//  |   TinyInt not Null Default '1'    |   MediumText not Null                 |(-,-)      <-- bunnies ftw
//  |   1 = Regex Mask and 2 = Exclude  |   Mask of Regex or Exclude            |o_(")(")
//  4) Roles    =========================================================================================================
//  |   MessageId                       |   RoleId                              |   EmoteId                             |
//  |-----------------------------------+---------------------------------------+---------------------------------------|
//  |   Unsigned BigInt not Null        |   Unsigned BigInt not Null            |   Unsigned BigInt not Null            |
//  |   Snowflake of message            |   Snowflake of role                   |   Snowflake of emote                  |
//  =====================================================================================================================
//
// EMIKO

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Data.Sqlite;

namespace BigSister.Database
{
    public class BotDatabase
    {
        SemaphoreSlim semaphoreSlim;

        /// <summary>Database connection datasource.</summary>
        string dataSource;

        public BotDatabase(string uri)
        {
            dataSource = $"Data Source={uri}";
            semaphoreSlim = new SemaphoreSlim(1, 1);
        }

        ~BotDatabase()
        {
            semaphoreSlim.Dispose();
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
                            `OriginalMessageId` BIGINT unsigned NOT NULL, -- Snowflake of original message.
                            `PinnedMessageId`   BIGINT unsigned NOT NULL  -- Snowflake of pinned aka reposted message.
                        );
                    ";

                command.ExecuteNonQuery();

                // --------------------------------
                // Reminder table.

                command.CommandText =
                    @"
                        CREATE TABLE `Reminders` (
	                        `Id`            TINYTEXT   NOT NULL, -- Reminder ID
	                        `Message`       MEDIUMTEXT         , -- Reminder message
	                        `TriggerTime`   TIMESTAMP  NOT NULL, -- Reminder trigger timestamp
	                        `Mentions`      MEDIUMTEXT NOT NULL  -- Whitespace separated mention strings
                        );
                    ";

                command.ExecuteNonQuery();

                // --------------------------------
                // Exclude system.

                command.CommandText =
                    @"
                        CREATE TABLE `Filter` (
                            `Type`      TINYINT unsigned NOT NULL DEFAULT '1', -- 1 REGEX 2 EXCLUDE
                            `String`    MEDIUMTEXT       NOT NULL              -- Mask of regex or exclude
                        );
                    ";

                command.ExecuteNonQuery();

                // --------------------------------
                // Role request.

                command.CommandText =
                    @"
                        CREATE TABLE `Roles` (
	                        `MessageId` BIGINT unsigned NOT NULL, -- Snowflake of message
	                        `RoleId`    BIGINT unsigned NOT NULL, -- Snowflake of role
	                        `EmoteId`   BIGINT unsigned NOT NULL  -- Snowflake of emote
                        );
                    ";
            }
        }
    }
}
