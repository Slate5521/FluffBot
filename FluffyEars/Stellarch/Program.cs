// Program.cs
// The main entry into the program, obviously. Here we have commands that relate to the program itself, such as the directory, CLI input processing,
// loading the settings before initiating the bot, and setting up the auditing system so we can immediately start auditing when the bot starts up.

#define DEBUG

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Stellarch
{
    public static class Program
    {
        public static class Files
        {
            /// <summary>The bot's directory.</summary>
            /// <remarks>Fuck how dotnet handles paths for Linux.</remarks>
            public static string BotDirectory
                => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            public static string BotSaveFileDirectory
                = Path.Combine(BotDirectory, @"settings");

            /// <summary>Authkey.json</summary>
            public static string AuthkeyFile
                = Path.Combine(BotSaveFileDirectory, @"authkey.txt");

            /// <summary>Webhooks.json</summary>
            public static string WebhookFile
                = Path.Combine(BotSaveFileDirectory, @"webhooks.json");

            /// <summary>Settings.json</summary>
            public static string SettingsFile
                = Path.Combine(BotSaveFileDirectory, @"settings.json");

            /// <summary>Settings_default.json</summary>
            public static string SettingsDefaultFile
                = Path.Combine(BotSaveFileDirectory, @"settings_defaults.json");

            /// <summary>Permissions.json</summary>
            public static string PermissionsFile
                = Path.Combine(BotSaveFileDirectory, @"settings.json");

            /// <summary>Permissions_default.json</summary>
            public static string PermissionsDefaultFile
                = Path.Combine(BotSaveFileDirectory, @"permissions_default.json");
        }




        static void Main(string[] args)
        {
            string authkey;

            // --------------------------------
            // Look through all the arguments.

            if (args.Length > 0)
            {
                if (args[0].Equals("-md5"))
                {   // Generate an MD5 file from something.
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendJoin(' ', args.Skip(1));

                    GenerateMD5AndQuit(stringBuilder.ToString());
                    
                    return; // ---------------------------------------------------------------------------------------------------- Quit the program.
                }
                else
                {   // We're not generating an MD5 and quitting, so let's look through the other args.
                    foreach(string arg in args)
                    {
                        switch(arg)
                        {
                            case "-noaudit":
                                BotSettings.SuppressAudit = true;
                                break;
                        } // end switch
                    } // end foreach
                } // end else
            } // end if

            // --------------------------------
            // Configure bot.

            bool success; // Our success variable that changes whenever we attempt to do something.

            Console.Write("Loading authkey... ");

            // Load authkey
            //authkey = LoadBotAuthkey(out success);
            //CLIAppendSuccess(success);

            Console.WriteLine("Loading bot settings...");

            LoadBotSettings(out success);
            CLIAppendSuccess(success);

            Console.WriteLine("Loading bot permissions...");

            LoadBotPermissions(out success);
            CLIAppendSuccess(success);

            Console.WriteLine("Loading bot webhooks...");

            LoadBotWebhooks(out success);
            CLIAppendSuccess(success);

            // --------------------------------
            // Initiate auditing.

            // TODO: I actually need to put auditing stuff here.

            // --------------------------------
            // Start bot.

            Console.WriteLine("Initiating bot...");

            //Bot.RunAsync(authkey).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>Load the authkey from the disk.</summary>
        /// <param name="success">Returns true if loading the authkey was successful.</param>
        private static string LoadBotAuthkey(out bool success)
        {
            string returnVal_authkey;

            using (var fs = File.OpenRead(Files.AuthkeyFile))
            {
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    returnVal_authkey = sr.ReadToEnd();
                }
            }

            success = returnVal_authkey.Length > 0;

            return returnVal_authkey;
        }

        /// <summary>Load the bot settings.</summary>
        /// <param name="success">Returns true if loading settings was successful.</param>
        private static void LoadBotSettings(out bool success)
            => success = BotSettings.LoadSettings();

        /// <summary>Load the bot permissions.</summary>
        /// <param name="success">Returns true if loading permissions was successful.</param>
        private static void LoadBotPermissions(out bool success)
            => success = BotSettings.LoadPermissions();

        /// <summary>Load the bot webhooks.</summary>
        /// <param name="success">Returns true if loading webhooks was successful.</param>
        private static void LoadBotWebhooks(out bool success)
        {
            throw new NotImplementedException();
        }

        /// <summary>Simply writes to the console based on a boolean.</summary>
        /// <param name="success">If the action was successful.</param>
        private static void CLIAppendSuccess(bool success)
        {
            if (success)
            {
                Console.WriteLine("Loaded!");
            }
            if (!success)
            {
                Console.WriteLine("Not loaded.");
            }
        }

        /// <summary>Just generate an md5 file and quit the program.</summary>
        /// <param name="file">The file to generate an md5 checksum of.</param>
        private static void GenerateMD5AndQuit(string file)
        {
            throw new NotImplementedException();
        }
    }
}
