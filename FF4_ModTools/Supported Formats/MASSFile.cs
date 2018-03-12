using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF4_ModTools
{

    public class MASSSubFile
    {
        public static readonly short NameMaxLength = 32;    // Max length of file name
        public static readonly char PaddingChar = '\x00';   // Byte to pad the remaining space with

        public UInt32 Offset;
        public UInt32 Size;
        public string FileName
        {
            get => this.fileName.TrimEnd('\x00');
            set => this.fileName = value.PadRight(NameMaxLength, PaddingChar);
        }
        public byte[] FileData;

        private string fileName;

        // TODO: convert to readonly property that is set when FileName is set.
        public string GetInternalType()
        {
            string extension = FileName.TrimEnd(".lz".ToCharArray());
            extension = extension.Substring(extension.LastIndexOf('.') + 1).ToUpper();

            if (extension == "NCGR" || extension == "NCBR")
            {
                // TODO: Check FileData for PNG Magic
                // If identified as PNG, return "PNG Image"
            }

            return extension;
        }
        
    }

    public struct MASSHeader
    {
        public static readonly char[] Magic = { 'S', 'S', 'A', 'M' };

        public UInt32 FileCount
        {
            get => this.fileCount;
            set
            {
                this.size = 40 * (int)value + 8;
                this.fileCount = value;
            }
        }

        public int Size
        {
            get => this.size;
        }

        private int size;
        private UInt32 fileCount;
    }

    public class MASSFile
    {
        public string Name
        {
            get => this.fileName;
        }
        public int BaseOffset
        {
            get => this.header.Size;
        }
        public UInt32 SubFileCount
        {
            get => this.header.FileCount;
        }
        public List<MASSSubFile> SubFiles;

        private string fileName;
        private MASSHeader header;

        public static MASSFile FromBinaryReader (ref BinaryReader reader)
        {
            char[] readerMagic = reader.ReadChars(4);

            if (!MASSHeader.Magic.SequenceEqual(readerMagic))
                throw new FileFormatException($"Expected Magic: {new string(MASSHeader.Magic)}, Found: {new string(readerMagic)}");

            MASSFile file = new MASSFile()
            {
                header = new MASSHeader
                {
                    FileCount = reader.ReadUInt32()
                },
                fileName = new FileInfo((reader.BaseStream as FileStream).Name).Name,
                SubFiles = new List<MASSSubFile>()
            };
            
            for (int i = 0; i < file.SubFileCount; i++)
            {
                file.SubFiles.Add(new MASSSubFile
                {
                    Offset = reader.ReadUInt32(),
                    Size = reader.ReadUInt32(),
                    FileName = new string(reader.ReadChars(MASSSubFile.NameMaxLength))
                });

                // TODO: Read subfile bytes as raw data and set FileData from there instead of using
                // BinaryReader stream.

                // Store current stream position
                long pos = reader.BaseStream.Position;

                // Seek to subfile offset
                reader.BaseStream.Seek(file.BaseOffset + file.SubFiles[i].Offset, SeekOrigin.Begin);

                // Copy subfile to FileData
                file.SubFiles[i].FileData = (reader.ReadBytes((Int32)file.SubFiles[i].Size));

                // Return to previous position
                reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            }

            return file;
        }
        
    }
}
