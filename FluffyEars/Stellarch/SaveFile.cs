using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace Stellarch
{
    class SaveFile<T>
    {
        const int MAX_BACKUPS = 5;

        private T SaveObject;

        /// <summary>Gets this file's name including the full directory path.</summary>
        public string FileName { public get; private set; }

        /// <summary>Gets this file's full directory.</summary>
        /// <returns>The directory or an empty string if the file does not exist.</returns>
        public string FileDirectory
        {
            get => File.Exists(FileName) ? Path.GetDirectoryName() : String.Empty;
        }

        public SaveFile(T @value, string fileName)
        {
            SaveObject = @value;
            FileName = fileName;
        }

        /// <summary>Load a file.</summary>
        /// <returns>Returns a <typeparamref name="T"/> with either default values or loaded values.</returns>
        public async Task<T> Load()
        {
            if(FileDirectory.Length > 0 && File.Exists(FileName))
            {   // Only continue if this is actually a valid file name.

                // Let's initiate our file load order and set the base file as the first file to load.
                var loadOrder = new List<string>();

                loadOrder.Add(FileName);
                loadOrder.AddRange(GenerateLoadOrder()); // Then we should add the detected backup files.

                bool successfulLoad = false;
                int i = 0;

                do
                {   // For every file string, let's try to load it...
                    string fileName = loadOrder[i++];

                    string fileContents;    // The contents of the file fileName.
                    string fileMD5;         // The MD5 of the file fileName.
                    string fileMD5Contents; // The contents of fileName_md5.

                    fileContents = ReadFile(fileName);
                    fileMD5      = CalculateFileMD5(fileName);
                    fileMD5Contents = ReadFile(AppendMD5Extension(fileName));

                    if (fileMD5.Equals(fileMD5Contents))
                    {   // Everything matches up, so let's load the file into memory.
                        


                        successfulLoad = true;
                    }

                } while (!successfulLoad);
            }
        }

        public async Task Save()
        {
            throw new NotImplementedException();
        }

        private static string AppendMD5Extension(string file)
            => $"{file}_md5";

        /// <summary>Generate a list of files, including the main load file and all the detected backups.</summary>
        private string GenerateLoadOrder()
        {
            // We want to get all the files in the directory that follow the naming convention FileName.*.ext and once we have those, we trim the 
            // list down so we only deal with the maximum amount of backups.
            string[] returnVal_allFiles = Directory
                .GetFiles(
                    path: FileDirectory, 
                    searchPattern: $"{Path.GetFileNameWithoutExtension(FileName)}.*.{Path.GetExtension(FileName)}",
                    searchOption: SearchOption.TopDirectoryOnly)
                .Take(MAX_BACKUPS);

            return returnVal_allFiles;
        }

        private static async Task<string> CalculateFileMD5(string file)
        {
            throw new NotImplementedException();
        }

        private static async Task<string> ReadFile(string file)
        {
            throw new NotImplementedException();
        }
    }
}
