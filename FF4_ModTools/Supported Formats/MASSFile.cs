using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF4_ModTools
{

    public struct MASSSubFile
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

        private string fileName;

        private byte[] FileData;

        internal string GetInternalType()
        {
            string extension = FileName.TrimEnd(".lz".ToCharArray());
            extension = extension.Substring(extension.LastIndexOf('.') + 1);

            return extension;
        }
    }

    public struct MASSHeader
    {
        public static readonly char[] Magic = { 'S', 'S', 'A', 'M' };

        public UInt32 fileCount;
        public List<MASSSubFile> subFiles;
        
    }

    public class MASSFile
    {
        private string fileName;
        private MASSHeader header;

        public UInt32 SubFileCount { get => this.header.fileCount; }
        public string Name { get => this.fileName; }
        public List<MASSSubFile> SubFiles { get => this.header.subFiles; }
        
        
        public static MASSFile FromBinaryReader (ref BinaryReader reader)
        {
            
            char[] readerMagic = reader.ReadChars(4);

            if (!MASSHeader.Magic.SequenceEqual(readerMagic))
                throw new FileFormatException($"Expected Magic: {MASSHeader.Magic}, Found: {readerMagic}");

            MASSFile file = new MASSFile()
            {
                header = new MASSHeader
                {
                    fileCount = reader.ReadUInt32(),
                    subFiles = new List<MASSSubFile>()
                },
                fileName = new FileInfo((reader.BaseStream as FileStream).Name).Name
            };
            
            for (int i = 0; i < file.SubFileCount; i++)
            {
                file.header.subFiles.Add(new MASSSubFile
                {
                    Offset = reader.ReadUInt32(),
                    Size = reader.ReadUInt32(),
                    FileName = new string(reader.ReadChars(MASSSubFile.NameMaxLength))
                });
            }

            return file;
        }
        
    }
}
