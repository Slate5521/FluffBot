using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SaveUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string file = args[0];
                if(File.Exists(file))
                {
                    string md5 = GetFileMD5(file);

                    using(StreamWriter sw = new StreamWriter(file + @".MD5", false))
                        sw.Write(md5);
                }
            }

            static string GetFileMD5(string file)
            {
                string fileContents = ReadFile(file);

                return GetHash(fileContents);
            }

            static string ReadFile(string file)
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

            static string GetHash(string contents)
            {
                byte[] hash;

                using (MD5 fileMD5 = MD5.Create())
                {
                    hash = fileMD5.ComputeHash(Encoding.UTF8.GetBytes(contents));
                }

                return Convert.ToBase64String(hash);
            }
        }
    }
}
