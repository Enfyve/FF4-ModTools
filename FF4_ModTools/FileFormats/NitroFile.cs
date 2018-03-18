using System.IO;

namespace FF4_ModTools.FileFormats
{
    public class NitroFile
    {
        public static readonly string Magic;

        public virtual string Name { get => fileName; set => fileName = value; }
        public virtual byte[] Data { get => data; set => data = value; }
        public bool IsCompressed { get => isCompressed; }

        protected string fileName;
        protected bool isCompressed = false;
        protected byte[] data;

        public string Extension => Name.Substring(Name.IndexOf('.') + 1);

        /// <summary>
        /// Reads from the current position in the binary reader to the end of the stream
        /// and stores the bytes into the data property.
        /// </summary>
        /// <param name="reader">The binary reader to read from.</param>
        public virtual NitroFile FromBinaryReader(BinaryReader reader)
        {
            this.data = reader.ReadBytes((int)reader.BaseStream.Length - (int)reader.BaseStream.Position);
            return this;
        }
    }
}