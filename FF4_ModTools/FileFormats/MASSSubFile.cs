using System;

namespace FF4_ModTools.FileFormats
{
    public struct MASSHeaderItem
    {
        public const short NameMaxLength = 32;    // Max length of file name
        public const char PaddingChar = '\x00';   // Byte to pad the remaining space with

        public UInt32 Offset;
        public UInt32 Size;
        public string Name
        {
            get => name.TrimEnd('\x00');
            set => name = value.PadRight(NameMaxLength, PaddingChar);
        }
        public bool Dirty;

        private string name;
    }

    /// <summary>
    /// Represents a file of unknown format that exists as a list item member of
    /// a parent MASSFile
    /// </summary>
    public sealed class MASSSubFile : NitroFile
    {
        public MASSHeaderItem Header;

        public override string Name { get => Header.Name; set => Header.Name = value; }

        public MASSSubFile()
        {
            MASSHeaderItem HeaderData = new MASSHeaderItem
            {
                Offset = 0,
                Size = 0,
                Name = "",
                Dirty = true
            };
        }

        // TODO: convert to readonly property that is set when FileName is set.
        public string GetInternalType()
        {
            string extension = Name.TrimEnd(".lz".ToCharArray());
            extension = extension.Substring(extension.LastIndexOf('.') + 1).ToUpper();

            if (extension == "NCGR" || extension == "NCBR")
            {
                // TODO: Check FileData for PNG Magic
                // If identified as PNG, return "PNG Image"
            }

            return extension;
        }

        public void ReplaceData(NitroFile newFile)
        {
            this.data = newFile.Data;
            Header.Size = (UInt32)data.Length;
            Header.Dirty = true;
        }
    }
}
