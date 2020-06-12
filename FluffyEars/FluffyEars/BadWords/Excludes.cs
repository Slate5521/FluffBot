// Excludes.cs
// This is a system that goes with the word filter system to exclude words, preventing them from triggering the word filter.

using System.Collections.Generic;


namespace FluffyEars.BadWords
{
    public static class Excludes
    {
        /// <summary>List of phrases to exclude.</summary>
        private static List<string> excludeList;

        /// <summary>The file to save Exclude information to.</summary>
        public const string BaseFile = @"excludes";
        /// <summary>The SaveFile object for this class.</summary>
        /// <see cref="SaveFile"/>
        private static SaveFile saveFile = new SaveFile(BaseFile);
        /// <summary>The lock object for this class' I/O operations.</summary>
        private static readonly object lockObj = (object)@"
               ((`\
            ___ \\ '--._
         .'`   `'    o  ) < boop!
        /    \   '. __.'
       _|    /_  \ \_\_
jgs   {_\______\-'\__\_\";

        #region Save/Load Methods
        
        /// <summary>Instantiate default values for this class.</summary>
        public static void Default()
        {
            // Default values in this case is an empty list.
            excludeList = new List<string>();

            ReorganizeList();
            Save();
        }

        /// <summary>Save the class to its save file.</summary>
        public static void Save()
            => saveFile.Save(excludeList, lockObj);

        /// <summary>Checks if the expected save file for this class can be loaded from.</summary>
        public static bool CanLoad() 
            => saveFile.IsExistingSaveFile();

        /// <summary>Loads the save file or instantiates default values if unable to load.</summary>
        public static void Load()
        {
            if (CanLoad())
            {
                excludeList = saveFile.Load<List<string>>(lockObj);
                ReorganizeList();
            } 
            else Default();
        }

        #endregion Save/Load Methods
        // ################################
        #region Public Methods

        /// <summary>Check if a phrase is in the exclude list.</summary>
        /// <returns>A boolean value indicating if the phrase is excluded.</returns>
        public static bool IsExcluded(string phrase) 
            => excludeList.Contains(phrase.ToLower());

        /// <summary>Check if a specified bad word in a message is excluded.</summary>
        /// <returns>A boolean value indicating if the phrase is excluded.</returns>
        public static bool IsExcluded(string msgOriginal, string badWord, int badWordIndex)
        {
            // The default return value is false because if there are no excluded words, then nothing can be excluded.
            bool returnVal = false;
            string msgLwr = msgOriginal.ToLower();

            if (excludeList.Count > 0) 
            {
                // Let's loop through every excluded word to check them against the list.
                foreach (var excludedPhrase in excludeList)
                {
                    if (returnVal)
                    {
                        break; // NON-SESE BREAK POINT! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! 
                    }

                    int excludedPhraseLength = excludedPhrase.Length;
                    int foundExcludeIndex = 0;
                    int scanIndex = 0; 
                    
                    do
                    {
                        if (scanIndex <= msgOriginal.Length)
                        {
                            foundExcludeIndex = msgLwr.IndexOf(excludedPhrase, scanIndex);
                        }
                        else
                        {
                            foundExcludeIndex = -1;
                        }

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

                            if (!returnVal)
                            {
                                scanIndex += foundExcludeIndex + excludedPhraseLength;
                            } 
                        } // end if
                    } while (foundExcludeIndex != -1 && !returnVal);
                } // end foreach
            } // end if

            return returnVal;
        }

        /// <summary>Add a phrase to the exclude list, preventing it from triggering the filter system.</summary>
        public static void AddPhrase(string phrase)
        {
            excludeList.Add(phrase.ToLower());

            ReorganizeList();
        }

        /// <summary>Remove a phrase from the exclude list, allowing it to trigger the filter system.</summary>
        public static void RemovePhrase(string phrase)
        {
            excludeList.Remove(phrase.ToLower());

            ReorganizeList();
        }

        /// <summary>Get all the phrases that are excluded.</summary>
        public static List<string> GetPhrases()
            => excludeList;

        /// <summary>Get the number of phrases that are excluded.</summary>
        public static int GetPhraseCount()
            => excludeList.Count;

        #endregion Public Methods
        // ################################
        #region Private Methods

        /// <summary>Reorganize the list in reverse alphabetical order.</summary>
        private static void ReorganizeList()
        {   // You may be rightfully wondering why we want to have this in reverse alphabetical order. The truth is that I have no idea. I am
            // documenting this months after making this implementation.
            excludeList.Sort();
            excludeList.Reverse();
        }

        #endregion Private Methods
    }
}
