// Excludes.cs
// This is a system that goes with the word filter system to exclude words, preventing them from triggering the word filter.

using System.Collections.Generic;


namespace FluffyEars.BadWords
{
    public static class Excludes
    {
        public const string BaseFile = "excludes";
        private static readonly object lockObj = (object)@"
               ((`\
            ___ \\ '--._
         .'`   `'    o  )
        /    \   '. __.'
       _|    /_  \ \_\_
jgs   {_\______\-'\__\_\";
        private static SaveFile saveFile = new SaveFile(BaseFile);

        /// <summary>List of words to exclude.</summary>
        private static List<string> excludeList;

        public static void Default()
        {
            // Default values in this case is an empty list.
            excludeList = new List<string>();
            ReorganizeList();
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
            {
                excludeList = saveFile.Load<List<string>>(lockObj);
                ReorganizeList();
            }
            else Default();
        }

        private static void ReorganizeList()
        {
            excludeList.Sort();
            excludeList.Reverse();
        }

        /// <summary>Check if a word is in the exclude list.</summary>
        public static bool IsExcluded(string word) => excludeList.Contains(word.ToLower());
        public static bool IsExcluded(string msgOriginal, string badWord, int badWordIndex)
        {
            // The default return value is false because if there are no excluded words, then nothing can be excluded.
            bool returnVal = false;
            string msgLwr = msgOriginal.ToLower();

            if (excludeList.Count > 0) 
            {
                // Let's loop through every excluded word to check them against the list.
                foreach (string excludedPhrase in excludeList)
                {
                    if (returnVal)
                        break; // NON-SESE BREAK POINT! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! 

                    int excludedPhraseLength = excludedPhrase.Length;

                    int foundExcludeIndex = 0, scanIndex = 0; do
                    {
                        if (scanIndex <= msgOriginal.Length)
                            foundExcludeIndex = msgLwr.IndexOf(excludedPhrase, scanIndex);
                        else
                            foundExcludeIndex = -1;

                        if (foundExcludeIndex != -1)
                        {
                            // A && B && C && D
                            // (A) the bad word starts at or after the found excluded word.
                            // (B) the bad word ends at or before the found excluded word ends.
                            // (C) exception protect: let's make sure the substring we want to get next is within bounds of the message.
                            // (D) the found excluded word contains the bad word.
                            returnVal = badWordIndex >= foundExcludeIndex &&
                                        badWordIndex + badWord.Length <= foundExcludeIndex + excludedPhraseLength &&
                                        foundExcludeIndex + excludedPhraseLength <= msgOriginal.Length &&
                                        msgLwr.Substring(foundExcludeIndex, excludedPhraseLength).IndexOf(excludedPhrase) != -1;

                            if(!returnVal)
                                scanIndex += foundExcludeIndex + excludedPhraseLength;
                        }

                    } while (foundExcludeIndex != -1 && !returnVal);

                }
            }

            return returnVal;
        }

        public static void AddPhrase(string phrase)
        {
            excludeList.Add(phrase.ToLower());
            ReorganizeList();
        }
        public static void RemovePhrase(string phrase)
        {
            excludeList.Remove(phrase.ToLower());
            ReorganizeList();
        }
        public static List<string> GetPhrases() => excludeList;
        public static int GetPhraseCount() => excludeList.Count;
    }
}
