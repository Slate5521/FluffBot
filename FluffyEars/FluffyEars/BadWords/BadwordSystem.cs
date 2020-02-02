// BadwordSystem.cs
// A static class that handles badwords.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluffyEars.BadWords
{
    public static class BadwordSystem
    {
        private static List<string> badWords;
        public const string BaseFile = "badwords";
        private static readonly object lockObj = (object)@"
         \\
          \\_
           (_)
          / )
   jgs  o( )_\_";
        private static SaveFile saveFile = new SaveFile(BaseFile);


        public static void Default()
        {
            badWords = new List<string>();
            Save();
        }
        public static void Save() => saveFile.Save<List<string>>(badWords, lockObj);
        public static bool CanLoad() => saveFile.IsExistingSaveFile();
        public static void Load()
        {
            if (CanLoad())
                badWords = saveFile.Load<List<string>>(lockObj);
            else Default();
        }


        /// <summary>Get all of the bad words.</summary>
        /// <returns>An array of strings with all the bad words.</returns>
        public static string[] GetBadWords() => badWords.ToArray();
        /// <summary>Check if the provided message has a bad word.</summary>
        /// <param name="msg">A message.</param>
        /// <returns>True if the message contains a bad word</returns>
        public static bool HasBadWord(string msg) => badWords.Any(msg.ToLower().Contains);
        /// <summary>Get the bad word within the message.</summary>
        /// <param name="msg">A message.</param>
        /// <returns>The bad word found in the message.</returns>
        public static string GetBadWord(string msg)
        {
            string returnVal = String.Empty;
            string msgLwr = msg.ToLower();

            // For each bad word that exists, check if this bad word resides within a lowercase version of the message.
            //
            // Okay, so the reason why I'm not using a more efficient search protocol--I can think of a couple--is because I doubt bad words are
            // going to be said several times a second. This method, while pretty slow and inefficient, may be only ever called a few times a day. I
            // will not be changing this to a more modern searching function.
            foreach (string badWord in badWords)
            {
                if (msgLwr.Contains(badWord))
                {
                    returnVal = badWord;
                    break;
                }
            }

            return returnVal;
        }
        /// <summary>Add a bad word to the bad word list.</summary>
        public static void AddBadWord(string badWord)
        {
            if (!badWords.Contains(badWord.ToLower()))
            {
                badWords.Add(badWord.ToLower());
                Save();
            }
        }
        /// <summary>Check if the word is a bad word.</summary>
        /// <returns>True if the word is a bad word.</returns>
        public static bool IsBadWord(string word)
        {
            return badWords.Contains(word.ToLower());
        }
        /// <summary>Remove a bad word from the bad word list.</summary>
        public static void RemoveBadWord(string badWord)
        {
            if (badWords.Contains(badWord))
            {
                badWords.Remove(badWord);
                Save();
            }
        }
    }
}
