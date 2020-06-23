using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stellarch
{
    class Program
    {
        public static Bot Bot;

        public static string BotDirectory
        {
            get
            {
                throw new NotImplementedException();
            }
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
                                Settings.SuppressAudit = true;
                                break;
                        } // end switch
                    } // end foreach
                } // end else
            } // end if

            // --------------------------------
            // Configure bot.

            bool success;

            Console.Write("Loading authkey... ");

            // Load authkey
            authkey = GetAuthkey(out success);
            CLIAppendSuccess(success);

            Console.WriteLine("Loading bot settings...");

            LoadBotSettings(out success);
            CLIAppendSuccess(success);

            Console.WriteLine("Loading bot permissions...");

            LoadBotPermissions(out success);
            CLIAppendSuccess(success);

            // --------------------------------
            // Start bot.

            Console.WriteLine("Initiating bot...");

            Bot = new Bot
                (
                    authkey: authkey
                );

            Bot.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static void LoadBotSettings(out bool success)
        {
            throw new NotImplementedException();
        }

        private static void LoadBotPermissions(out bool success)
        {
            throw new NotImplementedException();
        }

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

        private static void GenerateMD5AndQuit(string file)
        {
            throw new NotImplementedException();
        }

        private static string GetAuthkey(out bool success)
        {
            string returnVal_authkey;

            using (var fs = File.OpenRead(Path.Combine(BotDirectory, @"authkey")))
            {
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    returnVal_authkey = sr.ReadToEnd();
                }
            }

            success = returnVal_authkey.Length > 0;

            return returnVal_authkey;
        }
    }
}
