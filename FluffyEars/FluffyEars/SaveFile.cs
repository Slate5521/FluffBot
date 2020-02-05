// SaveFile.cs
// Each SaveFile is a reference to a dyad of files.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FluffyEars
{
    public class SaveFile
    {
        // When it's called base file, what this means is that this is its file name without anything fancy. For example, with every file, there will
        // be a...
        //
        // File.A
        // File.A.MD5
        // File.B
        // File.B.MD5
        //
        // ... And the file system will switch between each file and saving in the MD5.
        private string baseFileName;

        /// <summary>Get a string describing BaseFile.A</summary>
        private string BaseFileA => baseFileName + @".A";
        /// <summary>Get a string describing BaseFile.B</summary>
        private string BaseFileB => baseFileName + @".B";

        public SaveFile() { }
        public SaveFile(string baseFile)
        {
            baseFileName = baseFile;
        }

        /// <summary>Save information to savefile.</summary>
        /// <param name="lockObj">Locking object for class.</param>
        public void Save<T>(T saveData, object lockObj)
        {
            Save(
                json: JsonConvert.SerializeObject(saveData),
                lockObj: lockObj
                );
        }

        /// <summary>Save json information to savefile.</summary>
        /// <param name="lockObj">Locking object for class.</param>
        public void Save(string json, object lockObj)
        {
            string saveFile = GetNextSaveFile();


            // If there's no value, that means we need to default to bfA.
            if (saveFile.Equals(String.Empty))
                saveFile = BaseFileA;

            lock (lockObj)
            {
                // Let's save the SaveFile.
                using (var fs = File.OpenWrite(saveFile))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(json);
                        sw.Flush();
                    }
                }

                // Let's save the MD5.
                using (var fs = File.OpenWrite(GetMD5File(saveFile)))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(GetFileMD5(saveFile));
                        sw.Flush();
                    }
                }
            }
        }

        /// <summary>Get MD5 file string from FileBase.</summary>
        private string GetMD5File(string fileBase) => fileBase + @".md5";

        /// <summary>Load information from SaveFile.</summary>
        /// <typeparam name="T">Json serializable struct holding information.</typeparam>
        /// <param name="lockObj">Locking object for class.</param>
        public T Load<T>(object lockObj)
        {
            T returnVal;

            string loadFile = GetLoadFile(lockObj);

            if (!loadFile.Equals(String.Empty))
            {
                string fileContents;
                
                lock (lockObj)
                    fileContents = ReadFile(loadFile);

                if (!fileContents.Equals(String.Empty))
                    returnVal = (T)JsonConvert.DeserializeObject(fileContents, typeof(T));
                else throw new SaveFileException("SaveFile found, unable to load contents.");
                    
            }
            else throw new SaveFileException("Unable to load a SaveFile.");

            return returnVal;
        }

        /// <summary>Gets the save file we should save to.</summary>
        private string GetNextSaveFile()
        {
            string returnVal;

            bool bfAExists = File.Exists(BaseFileA);
            bool bfBExists = File.Exists(BaseFileB);

            // If both files exists, we need to see which one is the oldest one and return it.
            // ...
            if (bfAExists && bfBExists)
                returnVal = // If bfA creation time is greater than (more recent) than bfB, then bfA is the most recent, therefore bfB is the
                            // oldest, and we return that (1: bfB) instead. On the contrary, if bfA is NOT greater (less recent) than bfB, that means
                            // bfA is the oldest, and we return that (2: bfA) instead.
                    File.GetLastWriteTime(BaseFileA).Ticks >= File.GetLastWriteTime(BaseFileB).Ticks ?
                            /*(1)*/ BaseFileB :
                            /*(2)*/ BaseFileA;
            else if (bfAExists && !bfBExists) // If bfA exists AND bfB does NOT exist, return bfB.
                returnVal = BaseFileB;
            else if (!bfAExists && bfBExists) // If BfA does NOT exist and BfB exists, return bfA.
                returnVal = BaseFileA;
            else // Just return BaseFileA if there's nothing else.
                returnVal = BaseFileA;

            return returnVal;
        }

        /// <summary>Gets the savefile we should load from.</summary>
        /// <param name="lockObj">Locking object for class.</param>
        private string GetLoadFile(object lockObj)
        {
            string returnVal;

            bool bfAExists = File.Exists(BaseFileA);
            bool bfBExists = File.Exists(BaseFileB);

            if (!bfAExists && !bfBExists)
                throw new SaveFileException("Save files could not load: Not found.");

            string oldestFile; // String.Empty is our sentinel value.
            string newestFile;

            // If both files exists, we need to see which is the oldest and newest.
            if(bfAExists && bfBExists)
            {
                // If bfA creation time is greater (more recent) than bfB, it means bfA is the newest file. 
                bool bfANewest = File.GetLastWriteTime(BaseFileA).Ticks > File.GetLastWriteTime(BaseFileB).Ticks;

                if(bfANewest)
                {
                    newestFile = BaseFileA;
                    oldestFile = BaseFileB;
                } else
                {
                    newestFile = BaseFileB;
                    oldestFile = BaseFileA;
                }
            } 
            else if(bfAExists && !bfBExists)
            {
                newestFile = BaseFileA;
                oldestFile = String.Empty;
            }
            else if(!bfAExists && bfBExists)
            {
                newestFile = BaseFileB;
                oldestFile = String.Empty;
            }
            else
            {
                newestFile = String.Empty;
                oldestFile = String.Empty;
            }

            // Great. Now we have our newest file and oldest file with String.Empty as a sentinel value indicating it couldn't be found for some
            // reason. Hopefully it was found.

            // Now we need to get the MD5 file directories. If the file path does NOT equal String.Empty, we get the MD5 file, otherwise String.Empty
            string newestFileMD5 = !newestFile.Equals(String.Empty) ?
                                   GetMD5File(newestFile) :
                                   String.Empty           ;
            string oldestFileMD5 = !oldestFile.Equals(String.Empty) ?
                                   GetMD5File(oldestFile) :
                                   String.Empty           ;

            // Let's check if they exist really fast.
            bool newestFileMD5Exists = File.Exists(newestFileMD5);
            bool oldestFileMD5Exists = File.Exists(oldestFileMD5);
            bool newFileExists = !newestFile.Equals(String.Empty);
            bool oldFileExists = !oldestFile.Equals(String.Empty);

            string newFileTrueMD5; // True meaning this is the MD5 found in the .MD5 file, and this is what we want the MD5 to be.
            string oldFileTrueMD5;
            string newFileMD5;
            string oldFileMD5;

            lock (lockObj)
            {
                // Let's try load the MD5s of the newer files.
                if (newFileExists && newestFileMD5Exists)
                {
                    newFileTrueMD5 = ReadFile(newestFileMD5);
                    newFileMD5 = GetFileMD5(newestFile);
                }
                else
                {
                    newFileTrueMD5 = String.Empty;
                    newFileMD5 = String.Empty;
                }

                // Let's try load the MD5s of the older files.
                if (oldFileExists && oldestFileMD5Exists)
                {
                    oldFileTrueMD5 = ReadFile(oldestFileMD5);
                    oldFileMD5 = GetFileMD5(oldestFile);
                }
                else 
                { 
                    oldFileTrueMD5 = String.Empty;
                    oldFileMD5 = String.Empty;
                }
            }

            // Great. Now we have all the MD5s, so we need to compare them and see what's up.

            bool newFileIntegrity = CompareMD5(newFileTrueMD5, newFileMD5);
            bool oldFileIntegrity = CompareMD5(oldFileTrueMD5, oldFileMD5);

            bool newFileExisting = newFileExists && newestFileMD5Exists;
            bool oldFileExisting = oldFileExists && oldestFileMD5Exists;
            // If this scope is entered, great! All files exist and have integrity.
            if((newFileExisting && oldFileExisting) &&
               (newFileIntegrity && oldFileIntegrity))
            {
                returnVal = newestFile;
            } else // Uh oh, something fucked up.
            {
                // If this scope is entered, the newest file has the most integrity
                if (newFileExisting && newFileIntegrity)
                    returnVal = newestFile;
                // If this scope is entered, the oldest file has the most integrity
                else if (oldFileExisting & oldFileIntegrity)
                    returnVal = oldestFile;
                // If this scope is entered, everything is fucked.
                else throw new SaveFileException("Save files corrupt.");
            }

            return returnVal;
        }

        /// <summary>Read information from file.</summary>
        /// <param name="file">File to read from.</param>
        /// <returns>File contents.</returns>
        private string ReadFile(string file)
        {
            string returnVal;

            using (FileStream fs = File.OpenRead(file))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    returnVal = sr.ReadToEnd();
                }
            }
            return returnVal;
        }

        /// <summary>Get the file's MD5.</summary>
        /// <param name="file">File to get MD5 from.</param>
        /// <returns>The file's MD5.</returns>
        private string GetFileMD5(string file)
        {
            string fileContents = ReadFile(file);

            return GetHash(fileContents);
        }

        /// <summary>Gets a hash of a string in Base64.</summary>
        private string GetHash(string contents)
        {
            byte[] hash;
            
            using (MD5 fileMD5 = MD5.Create())
            {
                hash = fileMD5.ComputeHash(Encoding.UTF8.GetBytes(contents));
            }

            return Convert.ToBase64String(hash);
        }

        /// <summary>Compare two MD5s.</summary>
        /// <param name="md5_1">First MD5.</param>
        /// <param name="md5_2">Second MD5.</param>
        /// <returns>True if the two MD5s are equal.</returns>
        private bool CompareMD5(string md5_1, string md5_2)
        {
            StringComparer sc = StringComparer.OrdinalIgnoreCase;

            return sc.Compare(md5_1, md5_2) == 0;
        }

        // If either of these exists, it's an existing save file.
        /// <summary>Check if the save file exists.</summary>
        /// <returns>True if save file exists.</returns>
        public bool IsExistingSaveFile() => File.Exists(BaseFileA) || File.Exists(BaseFileB);
        
    }
}
