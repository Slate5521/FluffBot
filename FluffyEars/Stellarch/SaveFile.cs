using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography;

namespace Stellarch
{
    public class SaveFile<T>
    {
        /// <summary>Gets this file's name including the full directory path.</summary>
        public string FileName { get; private set; }

        /// <summary>Gets this file's full directory.</summary>
        /// <returns>The directory or an empty string if the file does not exist.</returns>
        public string FileDirectory
        {
            get => File.Exists(FileName) ? Path.GetDirectoryName(FileName) : String.Empty;
        }


        /// <summary>The maximum number of backups accepted.</summary>
        const int MAX_BACKUPS = 5;

        static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true    // I like human readable json.
        };

        
        public SaveFile(string fileName)
        {
            FileName = fileName;
        }

        #region Public Methods

        /// <summary>Load a file.</summary>
        /// <returns>Returns a <typeparamref name="T"/> with either a default value (likely null) or loaded values.</returns>
        /// <exception cref="FileLoadException">Thrown if an exception occurs upon file loading.</exception>
        public async Task<T> Load()
        {
            // This is being assigned to at the start so we can have a default value.
            T returnVal_object = default(T);

            if (FileDirectory.Length > 0 && File.Exists(FileName))
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
                    string md5FileName = AppendMD5Extension(fileName);

                    byte[] fileBytes;    // The save file's contents.
                    byte[] md5Bytes;     // The MD5 of the save file.
                    byte[] md5FileBytes; // The MD5 file's contents.
                    bool checksumsEqual; // If md5Bytes is equal to md5FileBytes.

                    // Begin reading files.
                    Task<byte[]> readFileBytes    = ReadFile(fileName);
                    Task<byte[]> readMd5FileBytes = ReadFile(md5FileName);
                    readFileBytes.Start();
                    readMd5FileBytes.Start();


                    // Calculate the save file's checksum.
                    readFileBytes.Wait();
                    fileBytes = readFileBytes.Result;
                    md5Bytes = CalculateMD5(fileBytes);


                    // Get the MD5 file's contents.
                    readMd5FileBytes.Wait();
                    md5FileBytes = readMd5FileBytes.Result;


                    // Compare checksums.
                    checksumsEqual = md5Bytes.SequenceEqual(md5FileBytes);

                    // At this point, we know if the checksums are equal or not. So if they are, let's load the file into memory.

                    if(checksumsEqual)
                    {   // Only continue if the checksums are equal.

                        // Now let's load the file.
                        try
                        {
                            using (var memStream = new MemoryStream(fileBytes))
                            {
                                returnVal_object = (T)await JsonSerializer.DeserializeAsync
                                    (
                                        utf8Json: memStream,
                                        returnType: typeof(T),
                                        options: SerializerOptions
                                    );
                            }   
                        } catch(JsonException e) 
                        {   // We want to catch the exception and throw our own so we have more information.
                            throw new FileLoadException("Save file version mismatch. Attempting to load a file from a version different than the " +
                                $"current one. File attempted to load: {fileName}.", e);
                        } // end catch
                    } // end if

                } while (!successfulLoad && i < loadOrder.Count);
                // Continue this loop only if we haven't loaded AND if we haven't exceeded the list boundaries.

                if (!successfulLoad && i == loadOrder.Count)
                {   // If we still haven't successfully loaded despite looking through multiple backups, we may have an issue.
                    throw new FileLoadException("Unable to load a file when files are known to exist. They may all be corrupt or inaccessible.");
                } // end if
            } // end do
            else
            {   // If this scope is entered, more than likely we just don't have anything. So we should just return the default value.
                returnVal_object = default(T);
            }

            return returnVal_object;
        }

        public async Task Save()
        {
            if (!Directory.Exists(Program.Files.BotSaveFileDirectory))
            {   // If the directory doesn't exist, let's make it.
                Directory.CreateDirectory(Program.Files.BotSaveFileDirectory);
            }

            var backupFiles = GenerateLoadOrder();

            if (backupFiles.Count() > 0)
            {   // If there are any backup files, we need to begin sorting them.

                if (backupFiles.Count() == 5)
                {   // There are more than 5, so we need to delete the oldest one.
                    string fileToDelete = backupFiles.Last();
                    backupFiles = backupFiles.Take(4);

                    File.Delete(fileToDelete);
                }

                // Let's try to copy the file...

                string newFileName =
                    Path.Combine(
                        Path.GetFileNameWithoutExtension(Path.GetFullPath(FileName)),
                        GetGuid(),
                        Path.GetExtension(FileName));

                string newFileNameMd5 = AppendMD5Extension(newFileName);

                // Let's save it.

                if (File.Exists(newFileName))
                {   // For the one in a million chance we get the same Guid.
                    File.Delete(newFileName);
                }

                if (File.Exists(newFileNameMd5))
                {
                    File.Delete(newFileNameMd5);
                }

                File.Copy(FileName, newFileName);
                File.Copy(AppendMD5Extension(FileName), newFileNameMd5);
            }

            // Now let's write the current data.

            //using(StreamWriter sw = new StreamWriter())
        }

        #endregion Public Methods
        #region Private Methods

        /// <summary>Read the contents of a file.</summary>
        private static async Task<byte[]> ReadFile(string file)
        {
            byte[] returnVal_contents;

            using(MemoryStream memStream = new MemoryStream())
            using(Stream stream = File.OpenRead(file))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {   // Continue reading while there's stuff to read.
                    await memStream.WriteAsync(buffer, 0, bytesRead);
                }

                returnVal_contents = memStream.ToArray();
            }

            return returnVal_contents;
        }

        /// <summary>Appends '_md5' to a file name.</summary>
        private static string AppendMD5Extension(string file)
            => $"{file}_md5";

        /// <summary>Generate a list of files, including the main load file and all the detected backups.</summary>
        private IEnumerable<string> GenerateLoadOrder()
        {
            // We want to get all the files in the directory that follow the naming convention FileName.GUID.ext and once we have those, we trim the 
            // list down so we only deal with the maximum amount of backups.
            return Directory.GetFiles(
                    path: FileDirectory,
                    searchPattern: $"{Path.GetFileNameWithoutExtension(FileName)}.*{Path.GetExtension(FileName)}",
                    searchOption: SearchOption.TopDirectoryOnly)
                .OrderBy(a => File.GetLastWriteTimeUtc(a).Ticks)    // Order by their creation date.
                .Take(MAX_BACKUPS);                                 // We only want 5.
        }

        /// <summary>Calculates an MD5 checksum based on bytes.</summary>
        private static byte[] CalculateMD5(byte[] bytes)
        {
            byte[] returnVal_checksum;

            using(var md5 = MD5.Create())
            using(var memStream = new MemoryStream(bytes))
            {
                returnVal_checksum = md5.ComputeHash(bytes);
            }

            return returnVal_checksum;
        }

        private static string GetGuid()
            => Guid.NewGuid().ToString();

        #endregion Private Methods
    }
}
