using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF4_ModTools.FileFormats
{
    class XBNFile : NitroFile
    {
        new public static readonly string Magic = "XBN ";
        new public Int32[] Data { get => data; }

        
        new private Int32[] data;

        public override NitroFile FromBinaryReader(BinaryReader br)
        {
            string readerMagic = new string(br.ReadChars(4));

            if (Magic != readerMagic)
                throw new FileFormatException($"Expected Magic: \"{Magic}\", Found: \"{readerMagic}\"");

            base.FromBinaryReader(br);

            return this;
        }
    }
}
