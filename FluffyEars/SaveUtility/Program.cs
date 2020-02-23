using System;
using System.IO;
using FluffyEars;

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
                    string md5 = SaveFile.GetFileMD5(file);

                    using(StreamWriter sw = new StreamWriter(file + @".MD5", false))
                        sw.Write(md5);
                }
            }
        }
    }
}
