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
using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using BigSister.Settings;
using System.Text;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;

namespace BigSister
{
    public static class Program
    {
        public static class Files
        {
            public static string ExecutableDirectory
            {
                get => AppDomain.CurrentDomain.BaseDirectory;
            }

            public const string SAVE_DIRECTORY = @"sav";
            public static string SaveDirectory
            {
                get => Path.Combine(ExecutableDirectory, SAVE_DIRECTORY);
            }

            const string IDENTITY_FILE = @"identity0.json";
            public static string IdentityFile
            {
                get => Path.Combine(ExecutableDirectory, SAVE_DIRECTORY, IDENTITY_FILE);
            }

            const string ANNOUNCEMENTS_FILE  = @"announcements.json";
            public static string AnnouncementsFile
            {
                get => Path.Combine(ExecutableDirectory, SAVE_DIRECTORY, ANNOUNCEMENTS_FILE);
            }
        }

        public static DiscordClient BotClient;

        public const string Prefix = @";";

        static Identity Identity;

        static void Main()
        {
            // ----------------
            // Initiate folders

            // ----------------
            // Load authkey and webhooks.
            Console.Write("Loading identity... ");
            Identity = LoadIdentity();
            Console.WriteLine("Found authkey and {0} webhook{1}.",
                Identity.Webhooks.Count,
                Identity.Webhooks.Count == 1 ? '\0' : 's');

            // ----------------
            // Load announcements file.
            Console.Write("Loading announcements... ");
            Announcements.Instance.Load();
            Console.WriteLine("Loaded announcements...");

            // ----------------
            // Run the bot.

            var botConfig = new DiscordConfiguration()
            {
                Token = Identity.Authkey,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            BotClient = new DiscordClient(botConfig);

            BotClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                EnableDefaultHelp = false,
                EnableDms = false,
                StringPrefixes = new string[] { @"!" }
            });

            // ----------------
            // Initialize static classes.

            WebhookDelegator.Initialize(Identity.Webhooks);

            Bot.RunAsync(BotClient).ConfigureAwait(false).GetAwaiter().GetResult();
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
    }
}
