using FluffyEars;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Stellarch
{
    static class BotSettings
    {
        public static bool SuppressAudit;


        // Bot Settings
        private static Dictionary<string, object> botSettings;
        private static SaveFile<Dictionary<string, object>> saveFile;


        // Permissions
        // Despite this being typed, I don't necessarily want to deserialize into a Permissions type because what if new commands are added, or 
        // commands are loaded? We'll have to fuck with everything to get the versions to match up again.
        private static Permissions permissions;
        private static SaveFile<Dictionary<string, object>> permissionsInternal;


        public static bool LoadSettings()
        {   // Let's just assume we're successful until we find evidence suggesting the contrary.
            bool returnVal_success = true;

            try
            {
                // Start the new bot settings save file.
                saveFile = new SaveFile<Dictionary<string, object>>(Program.Files.SettingsFile);

                // Let's deserialize whatever file the bot settings are in.
                botSettings = saveFile.Load().ConfigureAwait(false).GetAwaiter().GetResult();

                // Set default settings.
                Settings.DefaultBotSettings.SetDefaultSettings<Dictionary<string, object>>(ref botSettings);
            }
            catch
            {
                returnVal_success = false;
            }

            return returnVal_success;
        }
        public static bool LoadPermissions()
        {   // Let's just assume we're successful until we find evidence suggesting the contrary.
            bool returnVal_success = true;

            try
            {
                // Start the new bot settings save file.
                saveFile = new SaveFile<Dictionary<string, object>>(Program.Files.PermissionsFile);

                // Let's deserialize whatever file the bot settings are in.
                var permsData = saveFile.Load().ConfigureAwait(false).GetAwaiter().GetResult();

                // Now that we have the permission data let's initiate a new permission class
                permissions = new Permissions(permsData);

                // Set default settings.
                //Settings.DefaultBotSettings.SetDefaultSettings<Permissions>(ref permsData);
            }
            catch
            {
                returnVal_success = false;
            }

            return returnVal_success;
        }
    }
}
