using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;

namespace BigSister.Database
{
    class Database
    {

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
