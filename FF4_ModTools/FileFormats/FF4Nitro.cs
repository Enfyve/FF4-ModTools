using System.IO;

namespace FF4_ModTools.FileFormats
{
    public abstract class FF4Nitro
    {
        public string Name { get => this.fileName; }

        protected string fileName;
        protected bool isCompressed = false;

        public abstract FF4Nitro ReadFromBinary(ref BinaryReader br);
        public static FF4Nitro FromBinaryReader(ref BinaryReader br) => null;
    }
}
