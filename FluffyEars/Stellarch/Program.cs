// Program.cs
// The main entry into the program, obviously. Here we have:
//  - Commands that relate to the program itself such as saving/loading, SQL handling, auditing, logging, and CLI input processing.
//  - A space to load the settings before initiating the bot. 
//  - Initiating the auditing system so we can immediately start auditing when the bot starts up.
//
// EMIKO

#define DEBUG

using System;
using System.IO;
using BigSister.Settings;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO.Pipes;
using System.Threading;
using CommandLine;
using Microsoft.Data.Sqlite;

namespace BigSister
{
    public static class Program
    {
        protected static class Files
        {
            public static string ExecutableDirectory
            {
                get => AppDomain.CurrentDomain.BaseDirectory;
            }

            const string SAVE_DIRECTORY = @"sav";

            const string IDENTITY_FILE = @"identity.json";
            public static string IdentityFile
            {
                get => Path.Combine(ExecutableDirectory, SAVE_DIRECTORY, IDENTITY_FILE);
            }

            const string SETTINGS_FILE = @"settings.json";
            public static string SettingsFile
            {
                get => Path.Combine(ExecutableDirectory, SAVE_DIRECTORY, SETTINGS_FILE);
            }
        }
        protected class CLOptions
        {
            [Option('s', "sql",
                HelpText = "Process SQL file.",
                Required = false)]
            public bool ProcessSQLFile { get; set; }

            [Option('m', "md5",
                HelpText = "Process MD5 file.",
                Required = false)]
            public string InputMD5File { get; set; }

            [Option('o', "output",
                HelpText = "Output file.",
                Required = false)]
            public string OutputFile { get; set; }
        }

        public static BotSettings Settings;

        static SaveFile BotSettingsFile;
        static Identity Identity;

        static void Main(string[] args)
        {
            // ----------------
            // TODO: Process CLI
            if(ProcessCLI(args))
            {
                Environment.Exit(0);
            }

            bool loadSuccess;

            // ----------------
            // Load authkey and webhooks.
            Console.Write("Loading identity... ");
            Identity = LoadIdentity();
            Console.WriteLine("Found authkey and {0} webhook{1}.", 
                Identity.Webhooks.Count,
                Identity.Webhooks.Count == 1 ? '\0' : 's');

            // ----------------
            // Load bot settings.
            Console.Write("Loading settings... ");
            loadSuccess = LoadSettings(out Settings);
            Console.WriteLine(loadSuccess ? @"Successfully loaded" : @"No file - Using default values.");

            // ----------------
            // TODO: Initiate auditing.

            // ----------------
            // TODO: Initiate SQL stuff and load database items.

            // ----------------
            // TODO: Initiate logging.

            // ----------------
            // TODO: Run the bot.

            UpdateSettings(ref Settings.MaxTimeMonths, 7);
        }

        /// <summary>Load authkey and webhooks.</summary>
        static Identity LoadIdentity()
        {
            Identity identity_returnVal;

            if (File.Exists(Files.IdentityFile))
            {
                string identityFileContents;

                // Read the identity file's contents
                using (StreamReader sr = new StreamReader(Files.IdentityFile))
                {
                    identityFileContents = sr.ReadToEnd();
                }

                // Deserialize the object.
                identity_returnVal = JsonConvert.DeserializeObject<Identity>(identityFileContents);
            } 
            else
            {
                throw new FileNotFoundException("Unable to find identity.json.");
            }

            return identity_returnVal;
        }

        /// <summary>Update a setting and save if desired.</summary>
        /// <param name="setting">Reference of the setting to update.</param>
        /// <param name="newVal">New value of the setting.</param>
        /// <param name="save">If should be immediately save to disk.</param>
        public static void UpdateSettings<T>(ref T setting, T newVal, bool save = true)
        {
            setting = newVal;

            if(save)
            {
                SaveSettings();
            }
        }

        /// <summary>Save settings as is.</summary>
        public static void SaveSettings()
        {
            BotSettingsFile.Save<BotSettings>(Settings);
        }
        
        /// <summary>Load the bot's settings.</summary>
        /// Doesn't have to be asynchronous because I don't care what performance the bot has upon start.
        static bool LoadSettings(out BotSettings botSettings)
        {
            bool loadedValues_returnVal;

            BotSettingsFile = new SaveFile(Files.SettingsFile);

            // Check if it's an existing save file.
            if (BotSettingsFile.IsExistingSaveFile())
            {   // It's an existing file, so let's get the values.
                loadedValues_returnVal = true;

                botSettings = BotSettingsFile.Load<BotSettings>();
            }
            else
            {   // It's not an existing file, so let's use default values and then save them.
                botSettings = new BotSettings();
                loadedValues_returnVal = false;

                BotSettingsFile.Save<BotSettings>(botSettings);
            }

            return loadedValues_returnVal;
        }

        #region CLI

        /// <summary>Process CLI.</summary>
        /// <returns>Boolean indicating if program should end.</returns>
        private static bool ProcessCLI(string[] args)
        {
            bool processed_returnVal;

            var parseResult = Parser.Default.ParseArguments<CLOptions>(args);

            if (parseResult.Tag.Equals(ParserResultType.NotParsed))
            {   // Nothing was processed.
                processed_returnVal = false;
            }
            else
            {   // Something was processed.
                processed_returnVal = true;

                parseResult.WithParsed(
                    a =>
                    {
                        if(!(a.InputMD5File is null) && !(a.OutputFile is null) && 
                             a.InputMD5File.Length > 0 && a.OutputFile.Length > 0)
                        {   // Let's output an MD5 file.
                            GenerateMd5File(a.InputMD5File, a.OutputFile);
                        }
                        else if(!(a.OutputFile is null) &&
                                  a.ProcessSQLFile && a.OutputFile.Length > 0)
                        {   // Let's generate an MD5 file
                            GenerateSQLFile(a.OutputFile);
                        }
                    });
            }

            return processed_returnVal;
        }

        private static void GenerateMd5File(string inputFile, string outputFile)
        {
            throw new NotImplementedException();
        }

        private static void GenerateSQLFile(string outputFile)
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
	                        `MessageId` BIGINT unsigned NOT NULL, -- 'Snowflake of message'
	                        `RoleId`    BIGINT unsigned NOT NULL, -- 'Snowflake of role'
	                        `EmoteId`   BIGINT unsigned NOT NULL  -- 'Snowflake of emote'
                        );
                    ";
            }
        }

        #endregion CLI
    }
}
