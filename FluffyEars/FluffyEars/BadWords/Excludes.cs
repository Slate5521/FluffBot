// Excludes.cs
// This is a system that goes with the word filter system to exclude words, preventing them from triggering the word filter.

using System.Collections.Generic;


namespace FluffyEars.BadWords
{
    public static class Excludes
    {
        /// <summary>List of words to exclude.</summary>
        private static List<string> excludeList;
        public const string BaseFile = "wordexcludes";
        private static readonly object lockObj = (object)@"
               ((`\
            ___ \\ '--._
         .'`   `'    o  )
        /    \   '. __.'
       _|    /_  \ \_\_
jgs   {_\______\-'\__\_\";
        private static SaveFile saveFile = new SaveFile(BaseFile);

        public static void Default()
        {
            // Default values in this case is an empty list.
            excludeList = new List<string>();
            Save();
        }

        public static void Save()
        {
            saveFile.Save<List<string>>(excludeList, lockObj);
        }
        public static bool CanLoad() => saveFile.IsExistingSaveFile();
        public static void Load()
        {
            if (CanLoad())
                excludeList = saveFile.Load<List<string>>(lockObj);
            else Default();
        }

        /// <summary>Check if a word is in the exclude list.</summary>
        public static bool IsExcluded(string word) => excludeList.Contains(word.ToLower());
        public static bool IsExcluded(string message, int badWordIndex, int badWordLen)
        {
            bool returnVal = false;

            foreach(string excludedWord in excludeList)
            {
                if (!returnVal)
                {
                    int excludedIndex = message.ToLower().IndexOf(excludedWord);

                    // We've found an excluded word.
                    if(excludedIndex != -1)
                    {
                        int excludedLen = excludedWord.Length;

                        // What this does is it checks if the suspected bad word is within the boundaries of the excluded word. If it is, then we 
                        // know the word is excluded! Hooray.
                        returnVal = badWordIndex >= excludedIndex &&
                                    badWordIndex + badWordLen <= excludedIndex + excludedLen;
                    }
                }
            }

            return returnVal;
        }
        public static void AddWord(string word) => excludeList.Add(word.ToLower());
        public static void RemoveWord(string word) => excludeList.Remove(word.ToLower());
        public static List<string> GetWords() => excludeList;
        public static int GetWordCount() => excludeList.Count;
    }
}
