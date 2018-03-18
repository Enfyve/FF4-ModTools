using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF4_ModTools.FileFormats
{
    public sealed class MASSFile : NitroFile
    {
        new public static readonly string Magic = "SSAM";
        public const byte HeaderOffset = 8;
        public const byte HeaderItemSize = 40;

        public uint FileOffset { get => headerSize; }
        public uint SubFileCount
        {
            get => subFileCount;
            set
            {
                headerSize = HeaderOffset + (HeaderItemSize * value);
                subFileCount = value;
            }
        }
        public List<MASSSubFile> SubFiles;
        
        private uint headerSize;
        private uint subFileCount;
        
        public override NitroFile FromBinaryReader(BinaryReader reader)
        {
            string readerMagic = new string(reader.ReadChars(4));

            if (Magic != readerMagic)
                throw new FileFormatException($"Expected Magic: \"{Magic}\", Found: \"{readerMagic}\"");

            subFileCount = reader.ReadUInt32();
            headerSize = HeaderOffset + (HeaderItemSize * subFileCount);
            fileName = new FileInfo((reader.BaseStream as FileStream).Name).Name;
            SubFiles = new List<MASSSubFile>();

            for (int i = 0; i < subFileCount; i++)
            {
                SubFiles.Add(new MASSSubFile
                {
                    Header = new MASSHeaderItem
                    {
                        Offset = reader.ReadUInt32(),
                        Size = reader.ReadUInt32(),
                        Name = new string(reader.ReadChars(MASSHeaderItem.NameMaxLength))
                    }
                    
                });

                // TODO: Read subfile bytes as raw data and set FileData from there instead of using
                // BinaryReader stream.

                // Store current stream position
                long pos = reader.BaseStream.Position;

                // Seek to subfile offset
                reader.BaseStream.Seek(FileOffset + SubFiles[i].Header.Offset, SeekOrigin.Begin);

                // Copy subfile to FileData
                SubFiles[i].Data = (reader.ReadBytes((int)SubFiles[i].Header.Size));

                // Return to previous position
                reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            }

            return this;
        }

        internal void SetDirty(int index)
        {
            SubFiles
                .Where((x, i) => i >= index)
                .All(x => x.Header.Dirty = true);
        }

        internal void RecalculateHeader() => throw new NotImplementedException();

        internal byte[] GetData()
        {
            RecalculateHeader();
            return null;
        }
    }
}
