using FF4_ModTools.FileFormats;
using System;
using System.IO;

namespace FF4_ModTools
{
    public sealed class FileHandler
    {

        public static void GetFormat(String file)
        {
            FileInfo fileInfo = new FileInfo(file);

            Console.WriteLine(fileInfo.Extension);
        }

        public static T Open<T>(string file) where T : NitroFile, new()
        {
            // Open file for reading
            BinaryReader br = new BinaryReader(File.OpenRead(file));

            // Return new FF4Nitro created from specified Binary Reader
            return (T)(new T()).FromBinaryReader(br);
        }

        public static bool Save(string name, byte[] data)
        {
            FileStream outStream = new FileStream(name, FileMode.OpenOrCreate, FileAccess.Write);

            outStream.Write(data, 0, data.Length);

            outStream.Flush();
            outStream.Close();
            outStream.Dispose();

            return true;
        }
    }
}
