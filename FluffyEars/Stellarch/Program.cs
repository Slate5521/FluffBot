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
using Newtonsoft.Json;
using BigSister.Database;

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

            const string DATABASE_FILE = @"database.db";
            public static string DatabaseFile
            {
                get => Path.Combine(ExecutableDirectory, SAVE_DIRECTORY, DATABASE_FILE);
            }
        }

        public static BotSettings Settings;
        public static BotDatabase Database;

        static SaveFile BotSettingsFile;
        static Identity Identity;

        static void Main(string[] args)
        {
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
            Console.WriteLine(loadSuccess ? @"Successfully loaded" : @"No file - Used default values.");

            // ----------------
            // TODO: Initiate auditing.

            // ----------------
            // Initiate database.
            Console.Write("Looking for database file... ");
            string localDbPath = Path.GetRelativePath(Files.ExecutableDirectory, Files.DatabaseFile);
            if (File.Exists(Files.DatabaseFile)) // DB found
                Console.WriteLine("Found {0}!", localDbPath);
            else
            { // DB not found
                Console.WriteLine("No database - Instantiating default {0}.", localDbPath);
                BotDatabase.GenerateDefaultFile(Files.DatabaseFile);
            }

            Database = new BotDatabase(Files.DatabaseFile);


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

        private static void GenerateMd5File(string inputFile, string outputFile)
        {
            throw new NotImplementedException();
        }
    }
}
