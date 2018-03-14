using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FF4_ModTools.FileFormats;

namespace FF4_ModTools
{
    public sealed class FileHandler
    {

        public static T Open<T>(string file) where T : FF4Nitro, new()
        {
            // Open file for reading
            BinaryReader br = new BinaryReader(File.OpenRead(file));

            // Return new T created from specified Binary Reader
            return (T)(new T()).ReadFromBinary(ref br);
        }
    }
}
