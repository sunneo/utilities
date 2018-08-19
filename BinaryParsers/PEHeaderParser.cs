using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Utilities.BinaryParsers
{
    public class PEHeaderParser
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_DOS_HEADER
        {
            public ushort e_magic;   // Magic number
            public ushort e_cblp;    // Bytes on last page of file
            public ushort e_cp;      // Pages in file
            public ushort e_crlc;    // Relocations
            public ushort e_cparhdr; // Size of header in paragraphs
            public ushort e_minalloc; // Minimum extra paragraphs needed
            public ushort e_maxalloc; // Maximum extra paragraphs needed
            public ushort e_ss;    // Initial (relative) SS value
            public ushort e_sp;    // Initial SP value
            public ushort e_csum;  // Checksum
            public ushort e_ip;  // Initial IP value
            public ushort e_cs;  // Initial (relative) CS value
            public ushort e_lfarlc; // File address of relocation table
            public ushort e_ovno; // Overlay number
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] e_res; // Reserved words
            public ushort e_oemid; // OEM identifier (for e_oeminfo)
            public ushort e_oeminfo; // OEM information; e_oemid specific
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public ushort[] e_res2; // Reserved words
            public UInt32 e_lfanew;// File address of new exe header
        }

        public enum ImageSignatureTypes
        {
            IMAGE_DOS_SIGNATURE = 0x5A4D,// MZ
            IMAGE_OS2_SIGNATURE = 0x454E,    // NE
            IMAGE_OS2_SIGNATURE_LE = 0x454C,  // LE
            IMAGE_VXD_SIGNATURE = 0x454C,  // LE
            IMAGE_NT_SIGNATURE = 0x4550,  // PE00
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER
        {
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt32 BaseOfData;
            public UInt32 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public UInt32 SizeOfStackReserve;
            public UInt32 SizeOfStackCommit;
            public UInt32 SizeOfHeapReserve;
            public UInt32 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public IMAGE_DATA_DIRECTORY[] DataDirectory;
        }
        public struct IMAGE_NT_HEADERS
        {
            public UInt32 Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER OptionalHeader;
        }
        private object ReadRawType(BinaryReader r, Type t)
        {
            int rawsize = Marshal.SizeOf(t);
            byte[] rawData = r.ReadBytes(rawsize);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, 0, buffer, rawsize);
            object retobj = Marshal.PtrToStructure(buffer, t);
            Marshal.FreeHGlobal(buffer);
            return retobj;
        }
        IMAGE_DOS_HEADER ReadDosHeader(BinaryReader r)
        {
            return (IMAGE_DOS_HEADER)ReadRawType(r, typeof(IMAGE_DOS_HEADER));
        }
        IMAGE_NT_HEADERS ReadNTHeader(BinaryReader r)
        {
            return (IMAGE_NT_HEADERS)ReadRawType(r, typeof(IMAGE_NT_HEADERS));
        }
        public IMAGE_DOS_HEADER DosHeader { get; private set; }
        public IMAGE_NT_HEADERS NTHeader { get; private set; }
        public static ushort IMAGE_FILE_MACHINE_I386 = 0x014c;
        public static ushort IMAGE_FILE_MACHINE_IA64 = 0x0200;
        public static ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;
        private bool Read(String filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BufferedStream buffered = new BufferedStream(fs);
            BinaryReader r = new BinaryReader(buffered);
            DosHeader = ReadDosHeader(r);
            if (DosHeader.e_magic != (int)ImageSignatureTypes.IMAGE_DOS_SIGNATURE)
            {
                Valid = false;
                r.Close();
                return false;
            }
            buffered.Seek((long)DosHeader.e_lfanew, SeekOrigin.Begin);
            NTHeader = ReadNTHeader(r);
            if (NTHeader.Signature != (int)ImageSignatureTypes.IMAGE_NT_SIGNATURE)
            {
                r.Close();
                Valid = false;
                return false;
            }
            r.Close();
            return true;
        }
        bool Valid = false;
        public static PEHeaderParser FromFile(String filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);
            PEHeaderParser ret = new PEHeaderParser();
            ret.Read(filename);
            return ret;
        }
    }
}
