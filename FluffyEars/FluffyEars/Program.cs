// Program.cs



using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FluffyEars
{
    class Program
    {
        static void Main(string[] args)
        {
            bool runBot = true;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg)
                {
                    case "-md5": // Generate an md5 from a SaveFile file.

                        // The file name
                        var stringBuilder = new StringBuilder();
                        stringBuilder.AppendJoin(' ', args.Skip(i + 1));

                        GenerateMD5(stringBuilder.ToString());

                        // We don't wan to run teh bot after this.
                        runBot = false;

                        break;
                }
            }

            if (runBot)
            {
                Bot bot = new Bot();
                bot.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private static void GenerateMD5(string file)
        {
            string md5String = SaveFile.GetFileMD5(file);

            File.WriteAllText($"{file}.md5", md5String);
        }
    }
}

#region bunny!?!?!

/*
mmmmmmdmmmmmmmmmmmmmmmmmmmmmmddddho::::::::::::::---::::------------------------------------------------------------------------------------------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmdho::::::::::::::::::::::::::-------:-:--:::--------------:--------------------------------------------------------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmhs/::::::::::::::::::::::::::::::-:::::::::::::::::---------------------------------------------------------------------:------::::::-------------------------------------
mmNmmmmmmmmmmmmmmmmmmmmmmmmmdyo::::::::::::::::::::::::::::::--::::::::::::::::::::::---------------:-----:----:::-::----------------------------------------::::::::::---------------------------------
mmmmNNmmmmmmmmmmmmmmmmmmmmdyo/::-::::::::::::::::::::::::::::::::::::::::::::::::------------:::::---:::::::----------------------------------------------------::::::::--------------------------------
mmmmmmmNmmmmmmmmmmmmmmmmdy+:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::--::::::::::::::::-------------------------------------------------------:::::::-----------------:------------
mmmmmmmmmmmmmmmmmmmmmddy+:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::---------------------------------------------------------------:------------:::-----------
mmmmmmmmmmmmmmmmmmmmhs+::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::-::::----------------------::---::----------------------------------------:::--------::----------
mmmmmmmmmmmmmmmmmmds/:::::::::::::::::::::::::::::-::::::::::::::::::::::::::::::::::::::::::+oyyo/::::::::::::--:::-------------::--::::::-:-------------------::------------:-::::---------::---------
mmmmmmmmmmmmmmmmmmd/::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::/syhdhsoso::::///::::::::::---------------:::---:---:::::-----------------------------------------------------
mmmmmmmmmmmmmmmmmmd+::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::///:/shyyddhdmNhssyyyyyyyss+/:::::--::-------------:------------------------------------------------:---------------
mmmmmmmmmmmmmmmmmmd/:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::+hmhshdsoymddmdhyyssoosssssyhys/::::::::::---------------------------------::----------------------------------------
mmmmmmmmmmmmmmmmmmd+::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::ohmNmdysoshddhysossyyyydddhsooyhhs/:::::::::------------:-::--:------------:::----------------------------------------
mmmmmmmmmmmmmmmmmmdo:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::+dhydhsooshhyoosydmdhysoosydmhsooshhs+::::::::------::::::-::--:----:------------------------:-------------------------
mmmmmmmmmmmmmmmmmmdo::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::+hdsoooooossooshddhsoooooooooshddysssyhyo/:::::--------::::::::---------------------------:-----------------------------
mmmmmmmmmmmmmmmmmmds:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::/ymyooooooooooyddysooshdsoooooosyhhhsoosyhho/::::------::::--------------------------------------------------------------
mmmmmmmmmmmmmmmmmmdy/::::::::::::::::::::::::::::::::::::::::::::::::::::::::/+shhooooooooooossoooosdmyoooooooshddysoooooyhhs/::-----:::-----------------------::-:-------------------:::---------------
mmmmmmmmmmmmmmmmmmmh/::::::::::::::::::::::::::::::::::::::::::::::::::::::/+yyysooooooooooooooosydmhsoosoooooooshmhsooooooshhs/::::::-------------------------:--:-------------------------------------
mmmmmmmmmmmmmmmmmmmh/::::::::::::::::::::::::::::::::::::::::::::::::::::/ohhsoooooooooooooossydmNMNhsooosooooooooymmdhsoooooshdo::::::::--------------------------:------------------------------------
mmmmmmmmmmmmmmmmmmmd/:::::::::::::::::::::::::::::::::::::::::::-:::::::+hhssooooooooooyhddmNNMMMMMMMdysoooooooooooyNMMNmdhysoohmo::::---:--------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmd+:::::::::::::::::::::::::::::::::::::::::::::::::+yhyoooooooooooooyhmMMMMMMMMNMMMNdsooooooooosmMMMMMMMMNhoohmo::::-::--------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmdo:::::::::::::::::::::::::::::::::::::::::::::::/sdysssooooooooooooshhNMMMMMMd/sMMMMmyoooooooohNMMMmyNMMMNyoodd/::------------------------------------------------------------------
mmmmmmmmmmmmmmmmmdddo:::::::::::::::::::::::::::::::::::::::::::::/ohhsosdhoooooooooooyd+/NMMMMMMN::NMMMMmyssoooosdMMMMdomMMMMNyosmy/::-----------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmdo::::::::::::::::::::::::::::::::::::::::::://yhsoooymhooooooooooyh+`:NMMMMMMNysNMMMMMdysoooosmMMMMMMMMMMMMNsoymy::::---------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmdo::::::::::::::::::::::::::::::::::::::::::/shhsoossymhooooooooosh+.`-NMMMMMMMMMMMMMMMNdsoooosdNMMMMMMMMMMMMmsoydo:-::--------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmdo::::::::::::::::::::::::::::::::::::::::/shhsoossooymyssoooooooys.  `yMMMMMMMMMMMMMMNmmsooooohNMMMMMMMMMMMNddoshh+:::--------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmo::::::::::::::::::::::::::::::::::::::/shysooooossohmsoooooooosho`   .hMMMMMMMMMMMMMdsmyooooosdyhNMMMMMMMNo.+hoshh+:::-------------------------------------------------------------
mmmmmmmmmmmmmmmmmmdms/:::::::::::::::::::::::::::::::::::+ydhsssoooooooydhoooooooooyy:```  .omMMMMMMMMNdo:+dyooooooyo.:shdmmds:``/dsosdy/::-------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmy/::::::::::::::::::::::::::::::::/ohhysssssoooooosddsoooooooooyd+````  `-sdmmhyo:-...:hyyhhddhdmo.``....`...:/.-:+s/--:------------------------------------------------------------
mmmmmmmmmmmmmmmmmdddy/::::::::::::::::::::::::::::::/ohhysoooooooooooohdyoooooooo+/-::--.``` ```..``````:oyyydhyyhhhhy+..````````````.-:+:.`.::---------------------------------------------------------
mmmmmmmmmmmmmmmmmdddy/::::::::::::::::::::::::::::/shhysssssoooooooooymyooo+//:-...-...`.````````..--:+ohhyssshdmmdmds///+/::::::////---..-`.//:::------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmy/::::::::::::::::::::::::::/shyssssssssoooooooohho/////:..``...---.--/++//+oo++/oydho/://--/shs:./ohdyo+:-...``..--.`..-ohhysoso+++//:::-------------------------------------------
mmmmmmmmmmmmmmmmmmmmh/::::::::::::::::::::::::/shhsosssssooooooooo+/++soo:-.``````..--:///ss+:-..`.:shdyooso+::-:+ys/:::s+./ss/::/:/:-.``    `.:+oso+/+++oooooo++///::----------------------------------
mmmmmmmmmmmmmmmmmmmmh/::::::::::::::::::::::/ohhyssssooooooooooo//+o++:-.`...-://::---.``/-```.:+s+/-:o:`````````-s:``  /:  `//` ``.-:::/:-.`    `.:oo+::::-::://++oooo++/::----------------------------
mmmmmmmmmmmmmmmmmmmmd+::::::::::::::::::::/oyhyooosooooooooooo/:/+/:-...:///:-..``     ``.-//::oo-`  `:+`        -o:    /-   .+-        `.--:/::-.```-+so/:---------:::/++oo+//::-----------------------
mmmmmmmmmmmmmmmmmmmdd+::::::::::::::::::/+yhysooooooooooooosyo.--.``..``.``        ``-:+/:.``-/:`     .o:        -o:   .o.    .:`            `.-://::-../ss/::------------:::/++o+/::-------------------
mmmmmmmmmmmmmmmmmmmdd+:::::::::::::::::/yhhsosoooooooooosyyyo+:..``````        `.-:::-.``  ./:`       `/o.       -o-   /+      ``                 ``.--::+ddo:------------------::/+++/::---------------
mmmmmmmmmmmmmmmmmmddd+::::::::::::::::+ydysoooooooooooshdds/-.``           ``-://-.`      -/.         `:s/`      -+`  .y-                              ``.+dmy+/::------------------::///:--------------
mmmmmmmmmmmmmmmmmmdddo::::::::::::::/shhsssooooooosyhdhs+/.`            `.:///-`         `+.      ``----/o/.`   `//.-:/:`                                `-ymyosso+/:-----------------------------------
mmmmmmmmmmmmmmmmmmdddo:::::::::::::/sdhssosoossyhhdddhho-`           `.:+/:.`             `      .-.``   .-/+/--/so/-``                                 ./+yo::://+ss+/::-------------------------------
mmmmmmmmmmmmmmmmmmmddo::::::::::::/sdhsssssyyhhhyssdddy-`         `-://-`                                   ``...``          `.--`                    `:yyss/------:/+oo+/------------------------------
mmmmmmmmmmmmmmmmmmmmds:::::::::::/odhsssyhdhyssooooshy:`       `-://.`                                                    `://-.`                 `..-+ydho/----------:/+o+/:---------------------------
mmmmmmmmmmmmmmmmmmmddy:::::::::::+hdhhdhyysoooosooosdy+:`   `-://-`                                                     ./sys+---.`````````````.-/osso+/+/:------------:--/++/:-------------------------
mmmmmmmmmmmmmmmmmmmmdy::::::::::+ymmhysooooooooooooymmmo.`.:+/-`                           `.----::-...`               :ysoooooossosooss+////++ohmds/-----------------------:::-------------------------
mmmmmmmmmmmmmmmmmmmddy:::::::/oyhmmsoooooooooooooooyNmds:++-`                         `.-//+sssoo/-.``                :dsoooooooooooooymy/::::::+o/:----------------------------------------------------
mmmmmmmmmmmmmmmmmmmddy::::/oyyo/odhoooooooooooooooosyydmsos:.  `                ``.-/+oo+/::::::/oos:                .hhooooooooooooooshd+:-------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmdh+/+sso+:::+dhoooooooooooooooosyhhdshdhs-``        ```.-::+oooo//::--------::/sh/.``            /dyssoooooooooooooydo:-------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmhso/::::::+dhoooooooooooooosydysoydmhssyso+//::/++++o+o+//:::-----------::/syhddhso+/:-..```..:ydhddhysoooooooooosdy/::-----------------------------------------------------------
mmmmmmmmmmmmmmmmmmNNNdo::--:::::+dhooooooooooooshdysoooosysoooosyddo+//::::::::::-------------:/sddhhhhhhhddddddhhhhdddddddddyoooooooooohd+:------------------------------------------------------------
mmmmmmmmmmmmmmmNNmmmdh+:::::::::/hdooooooooooshdyoooooooooooooooyms:-:--:::::::::------------:::odddhhhhhhdddddddddddddddddddyoooooooooohd+:------------------------------------------------------------
mmmmmmmmmmmmmmmmmmddmdo:::::::::/ydsooooooooydhooooooooooooooooymh+:----::::::::-:::--:------::+sdddddhhhdddddddddddddddddddddhsoooooooohd+:------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmdo:::::::::/ydsooooooshhsooooooooooooooooydd+::::::::::::::--:--------::+ydddddmmmdddddddddddddddddhddhhhddhooooooohh+:------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmds::::::::::smyooooooyysooooooooooooooooohms/:::::::::::::::::------:::+ydddhhhdmmmmmmmdddddddddddddddddddmhoooooosdy/:------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmds::::::::::omhooooooooooooooooooooooooosmd+:::::::::::::::::::-----:::/yddddhhdddmmmmmdddddddddddddddddddmdsoooooydo:-------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmds::::::::::/ydsoooooooooooooooooooooooodmd+:::::::::::::::::::::::---:/yddmddddddddddddddddddddddddddddddmmdyooosdh/--------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmdy:::::::::::+dhsooooooooooooooooooooooddydo::::::::-----:::::::::::::/yddddddmmdddddddddddddddmmmmmddddddddmyoooyds::-------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmy:::::::::::/ydsoooooooooooooooooooooydysds:::::::::::-::::::::::::::/sdddddddddddmddddddddddddddddddddddmdyooosdy/::-------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmdmh/:::::::::::+hdsooooooooooooooooooosdyosds:::::::::::::::::::::::::::/ohddddddddddddddddddddddddddddddddmhsoosddo:::-------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmdh+::::::::::::odhooooooooooooooooooossoosds:::::::::::::::::::::::::::+yddddmmdddddddddddddddddddddddhdhhddsooydo:::--------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmdd+:::::::::::::odhoooooooooooooooooooooosds::::::::::::::::::::::::/+sdmddddddmmmdddddddddddddddddddddhhdmdsoyds::::---:::--------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmddo::::::::::::::smyooooooooooooooooooooosds::::::::::::::::::::::://odmmddddddddddddddddddddddddddddddddmddddmy/::::::-:----:-----------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmdds::::::::::::::/sdyooooooooooooooooooooyds:::::::::::::::::::::::/+ydmmmddddddddddddddddddddmmmmdmmmmddddhddh+:::::::------------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmdds::::::::::::::::ydyoooooooooooooooooooyds:::::::::::::::::::::::+hddddddmmmddddddddddddddddmmmmmddddddhhhdds/::::::::::::::-----------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmdds:::::::::::::::::ohhsoooooooooooooooooyms::::::::::::::::::::::/odmddddddddddddddddddddddddddddddddddddddddy+:::::::::::--------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmddy::::::::::::::::::/ydyooooooooooooooooyms:::::::::::::::::::::/osddmmdddddddddhhhhhhhddddddddddddddmddddddddds/:::::::----------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmddy/::::::::::::::::::/sdhsooooooooooooooyms::::::::::::::::::::/ohmddddddddddddddddddddddddddddddddddddddddddddy+::::::::-------------------------------------------:-------------
mmmmmmmmmmmmmmmmmmmmmddh+::::::::::::::::::::+ydhsooooooooooooyds:::::::::::::::::::/oydmddddddddddddddddddddddddddddddddddddddddddddy+:::::::::--------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmdh+::::::::::::::::::::::ohdyoooooooooooydy/:::::::::::::::::/ohddddmmmdddddddddddddddddddddddddddddddddddddddddyo/:::::::--------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmdh+:::::::::::::::::::::::/smhsoooooooooymh+//:::::::::::::::/ohdddddddmmmdddddddddddddddddddddddddddddddmdddddddy+:----:---------------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmmh+:::::::::::::::::::::::::+ydhsoooooooymh/:/::::::::::::::::/ohdmddddddddmmmmddddddddddddddddddddddmmdddddddddhs/::::::::--:::-----::-------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmmh+:::::::::::::::::::::::::::ohdysoooooymh/:/:::::::::::::::/shddddmmdddddddddddddddddddddddddddmdddddddddddddy+::::::::::--::::---:::-------::----:---------::------------------
mmmmmmmmmmmmmmmmmmmmmmmh+::::::::::::::::::::::::::::/shhsooooymh+//:::::::::::::::+hmddddddddmmddddddddddddddddddddddddddddddddddmmh+:::::::::::::::-:----:---:-------------::-------------------------
mmmmmmmmmmmmmmmmmmmmmmmdo::::::::::::::::::::::::::::::+yhyssoymy/:/:::::::::::::::+shmdddddddddddddddmdddddddddddddddddddddddddddddy+::::::::::::---:--------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmmdo::::::::::::::::::::::::::::::::+hhssddo:::::::::::::::://ohdmmmmmddddddddddddddddddddddddddddddddddddddddhyo//:::::::::::::::----------------------:--------------------------
mmmmmmmmmmmmmmmmmmmmmmmdo:::::::::::::::::::::::::::::::::/ssys+::::::::::::::::://sdmdddddmmmmmddddddddddddddddddddddddddddddddmmmmdddhs/:::::::::::::-------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmmdo:::::::::::::::::::::::::::::::::::::::::::::::::::::::/+shdmmddddddddddddmdddddddddddddddddddddddddmmmmmmddddmh+:::::::::::::-------------------------------------------------
mmmmmmmmmmmmmmmmmmmmmmmds/::::::::::::::::::::::::::::::--::::::::::::::::::////sdddddmmmmmdddddddddddddddddddddddddddddddddmmmmddddddmdy+:::::::-::-:::-:----::::---:----------------------------------
mmmmmmmmmmmmmmmmmmmmmmmdy/::::::::::::::::::::::::::::::::::::::::::::::::::/:/ommdddddddddmmdddddddddddddddddddddddddddddmmmddddddddmddddy+:::::::::::::::--:::-:--------------------------------------
mmmmmmmmmmmmmmmmmmmmmmmdy/::::::::::::::::::::::::::::::::::::::::::::::::::::/odmmddddddddddddmmddddddddhddddddddddddmmmdddddddddmmdddddddo:::::::::::::::-:::::--------------:------------------------
*/

#endregion !!!!!!!!!!!!!!!!!!!!