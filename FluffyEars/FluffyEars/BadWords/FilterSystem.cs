// BadwordSystem.cs
// A static class that handles badwords.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FluffyEars.BadWords
{
    public static class FilterSystem
    {
        private static List<string> filterList;
        public const string BaseFile = "filter";
        private static readonly object lockObj = (object)@"
         \\
          \\_
           (_)
          / )
   jgs  o( )_\_";
        private static SaveFile saveFile = new SaveFile(BaseFile);
        private static string regexPattern = String.Empty; // All of filterList in a pattern.

        public static void Default()
        {
            filterList = new List<string>();
            Save();
        }
        public static void Save()
        {
            saveFile.Save<List<string>>(filterList, lockObj);

            UpdatePatternString();
        }
        public static bool CanLoad() => saveFile.IsExistingSaveFile();
        public static void Load()
        {
            if (CanLoad())
            {
                filterList = saveFile.Load<List<string>>(lockObj);
                UpdatePatternString();
            }
            else Default();
        }

        private static void UpdatePatternString()
        {
            StringBuilder sb = new StringBuilder();

            foreach(string pattern in filterList)
            {
                sb.Append(pattern);

                // If this is not the last filter word, add an | separator. 
                if (!filterList.Last().Equals(pattern))
                    sb.Append('|');
            }

            regexPattern = sb.ToString();
        }

        public static bool IsWord(string word) => filterList.Contains(word);
        public static List<string> GetWords() => filterList; 

        public static void AddWord(string word) => filterList.Add(word);
        public static void RemoveWord(string word) => filterList.Remove(word);

        static RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
        public static List<string> GetBadWords(string message)
        {
            List<string> returnVal = new List<string>(); // Our sentinel value for no bad word is an empty List<string>.

            if (filterList.Count > 0)
            {
                MatchCollection mc = Regex.Matches(message, regexPattern, regexOptions);

                // Great, we found something!
                if (mc.Count > 0)
                {
                    foreach (Match match in mc)
                    {
                        string possibleBadWord = match.Value;

                        if (!Excludes.IsExcluded(message, match.Index, match.Length))
                            returnVal.Add(possibleBadWord);
                    }
                }
            }

            return returnVal;
        }
    }
}
