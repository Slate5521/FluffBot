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
using System.Linq;
using System.Reflection;
using System.Text;

namespace BigSister
{
    public static class Program
    {
        static void Main(string[] args)
        {
            // Process CLI
            if(ProcessCLI(args))
            {
                Environment.Exit(0);
            }
        }

        /// <summary>Process CLI.</summary>
        /// <returns>Boolean indicating if program should end.</returns>
        private static bool ProcessCLI(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
