using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.BinaryParsers
{
    public class PEHeaderParser
    {
        #region Interfaces for PDBInFile
        public class IPDBDataFetcher
        {
            public virtual String GetFileName() { return ""; }
            public virtual IPDBDataFetcher Init(byte[] rawData) { return this; }
        }
        public class PDB20Fetcher : IPDBDataFetcher
        {
            public PEHeaderParser Parent;
            public NB10PdbData20 RawData;
            public override String GetFileName()
            {
                return RawData.PdbFileName.ToString();
            }
            public override IPDBDataFetcher Init(byte[] RawData)
            {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(RawData)))
                {
                    this.RawData.Header.charSignature = reader.ReadBytes(4);
                    this.RawData.Offset = reader.ReadUInt32();
                    this.RawData.Signature = reader.ReadUInt32();
                    this.RawData.Age = reader.ReadUInt32();
                    this.RawData.PdbFileName = new StringBuilder();
                    while (true)
                    {
                        byte b = reader.ReadByte();
                        if (b <= 0) break;
                        this.RawData.PdbFileName.Append((char)b);
                    }
                }
                return this;
            }
        }
        public class PDB70Fetcher : IPDBDataFetcher
        {
            public PEHeaderParser Parent;
            public RSDSPdbData RawData;
            public override String GetFileName()
            {
                return RawData.szFileName.ToString();
            }
            public override IPDBDataFetcher Init(byte[] RawData)
            {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(RawData)))
                {
                    this.RawData.Signature.charSignature = reader.ReadBytes(4);
                    this.RawData.GUID.time_low = reader.ReadUInt32();
                    this.RawData.GUID.time_mid = reader.ReadUInt16();
                    this.RawData.GUID.time_hi_and_version = reader.ReadUInt16();
                    this.RawData.GUID.clock_seq_hi_and_reserved = reader.ReadByte();
                    this.RawData.GUID.clock_seq_low = reader.ReadByte();
                    this.RawData.GUID.node = reader.ReadBytes(6);
                    this.RawData.Age = reader.ReadUInt32();
                    this.RawData.szFileName = new StringBuilder();
                    while (true)
                    {
                        byte b = reader.ReadByte();
                        if (b <= 0) break;
                        this.RawData.szFileName.Append((char)b);
                    }
                }
                return this;
            }
        }
        #endregion


        #region Data Types
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
        public enum IMAGE_FILE_CHARACTERISTIC
        {
            E_IMAGE_FILE_RELOCS_STRIPPED = 0x0001,
            E_IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002,
            E_IMAGE_FILE_LINE_NUMS_STRIPPED = 0x0004,
            E_IMAGE_FILE_LOCAL_SYMS_STRIPPED = 0x0008,
            E_IMAGE_FILE_AGGRESSIVE_WS_TRIM = 0x0010,
            E_IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020,
            E_RESERVED = 0x0040,
            E_IMAGE_FILE_BYTES_REVERSED_LO = 0x0080,
            E_IMAGE_FILE_32BIT_MACHINE = 0x0100,
            E_IMAGE_FILE_DEBUG_STRIPPED = 0x0200,
            E_IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP = 0x0400,
            E_IMAGE_FILE_NET_RUN_FROM_SWAP = 0x0800,
            E_IMAGE_FILE_SYSTEM = 0x1000,
            E_IMAGE_FILE_DLL = 0x2000,
            E_IMAGE_FILE_UP_SYSTEM_ONLY = 0x4000,
            E_IMAGE_FILE_BYTES_REVERSED_HI = 0x8000
        }
        public enum IMAGE_DLL_CHARACTERISTIC
        {
            E_IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA = 0x0020,
            E_IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE = 0x0040,
            E_IMAGE_DLLCHARACTERISTICS_FORCE_INTEGRITY = 0x0080,
            E_IMAGE_DLLCHARACTERISTICS_NX_COMPAT = 0x0100,
            E_IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,
            E_IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,
            E_IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,
            E_IMAGE_DLLCHARACTERISTICS_APPCONTAINER = 0x1000,
            E_IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,
            E_IMAGE_DLLCHARACTERISTICS_GUARD_CF = 0x4000,
            E_IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
        }
        public enum ImageDebugType
        {
            E_IMAGE_DEBUG_TYPE_UNKNOWN = 0,
            E_IMAGE_DEBUG_TYPE_COFF = 1,
            E_IMAGE_DEBUG_TYPE_CODEVIEW = 2,
            E_IMAGE_DEBUG_TYPE_FPO = 3,
            E_IMAGE_DEBUG_TYPE_MISC = 4,
            E_IMAGE_DEBUG_TYPE_EXCEPTION = 5,
            E_IMAGE_DEBUG_TYPE_FIXUP = 6,
            E_IMAGE_DEBUG_TYPE_OMAP_TO_SRC = 7,
            E_IMAGE_DEBUG_TYPE_OMAP_FROM_SRC = 8,
            E_IMAGE_DEBUG_TYPE_BORLAND = 9,
            E_IMAGE_DEBUG_TYPE_RESERVED10 = 10,
            E_IMAGE_DEBUG_TYPE_CLSID = 11,
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
            #region named DataDirectory
            public IMAGE_DATA_DIRECTORY ExportTable
            {
                get
                {
                    return DataDirectory[0];
                }
            }
            public IMAGE_DATA_DIRECTORY ImportTable
            {
                get
                {
                    return DataDirectory[1];
                }
            }
            public IMAGE_DATA_DIRECTORY ResourceTable
            {
                get
                {
                    return DataDirectory[2];
                }
            }
            public IMAGE_DATA_DIRECTORY ExceptionTable
            {
                get
                {
                    return DataDirectory[3];
                }
            }
            public IMAGE_DATA_DIRECTORY CertificateTable
            {
                get
                {
                    return DataDirectory[4];
                }
            }
            public IMAGE_DATA_DIRECTORY BaseRelocationTable
            {
                get
                {
                    return DataDirectory[5];
                }
            }
            public IMAGE_DATA_DIRECTORY DebuggingInformation
            {
                get
                {
                    return DataDirectory[6];
                }
            }
            public IMAGE_DATA_DIRECTORY ArchitectureSpecificData
            {
                get
                {
                    return DataDirectory[7];
                }
            }
            public IMAGE_DATA_DIRECTORY GlobalPointerRegisterRelativeVirtualAddress
            {
                get
                {
                    return DataDirectory[8];
                }
            }
            public IMAGE_DATA_DIRECTORY ThreadLocalStorage
            {
                get
                {
                    return DataDirectory[9];
                }
            }
            public IMAGE_DATA_DIRECTORY LoadConfigurationTable
            {
                get
                {
                    return DataDirectory[10];
                }
            }
            public IMAGE_DATA_DIRECTORY BoundImportTable
            {
                get
                {
                    return DataDirectory[11];
                }
            }
            public IMAGE_DATA_DIRECTORY ImportAddressTable
            {
                get
                {
                    return DataDirectory[12];
                }
            }
            public IMAGE_DATA_DIRECTORY DelayImportDescription
            {
                get
                {
                    return DataDirectory[13];
                }
            }
            public IMAGE_DATA_DIRECTORY CLRHeader
            {
                get
                {
                    return DataDirectory[14];
                }
            }
            public IMAGE_DATA_DIRECTORY Reserved
            {
                get
                {
                    return DataDirectory[15];
                }
            }
            #endregion
        }

        public class IMAGE_SECTION_HEADER
        {
            public byte[] NameBytes;
            public String Name
            {
                get
                {
                    return Encoding.ASCII.GetString(NameBytes);
                }
            }
            public uint PhysicalAddress;
            public uint VirtualSize
            {
                get
                {
                    return PhysicalAddress;
                }
                set
                {
                    PhysicalAddress = value;
                }
            }

            public uint VirtualAddress;
            public uint SizeOfRawData;
            public uint PointerToRawData;
            public uint PointerToRelocations;
            public uint PointerToLinenumbers;
            public ushort NumberOfRelocations;
            public ushort NumberOfLinenumbers;
            public uint Characteristics;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_NT_HEADERS
        {
            public UInt32 Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER OptionalHeader;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DebugDataDirectory
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            // debug type
            public uint Type;
            public uint SizeOfData;
            public uint AddressOfRawData;
            // use Pdb to map it
            public uint PointerToRawData;
        }
        
        /// RSDS
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DebugTableSignature
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] charSignature;
            public int Value
            {
                get
                {
                    var reader = new BinaryReader(new MemoryStream(charSignature));
                    reader.BaseStream.Position = 0;
                    return reader.ReadInt32();
                }
                set
                {
                    new BinaryWriter(new MemoryStream(charSignature)).Write(value);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NB10PdbData20
        {
            public DebugTableSignature Header;
            public uint Offset;
            public uint Signature;       // seconds since 01.01.1970
            public uint Age;             // an always-incrementing value 
            public StringBuilder PdbFileName;  // zero terminated string with the name of the PDB file 
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RSDSPdbDataGUID
        {
           public uint time_low;
           public ushort time_mid;
           public ushort time_hi_and_version;
           public byte clock_seq_hi_and_reserved;
           public byte clock_seq_low;
           [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
           public byte[] node;
        }

	    /* PDB7.0 */
        public struct RSDSPdbData
        {
            public DebugTableSignature Signature; // RSDS
            public RSDSPdbDataGUID GUID;
            public uint Age;
            public StringBuilder szFileName; //zero terinated UTF8 path and filename
        }

        #endregion

        #region Private Functions
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
        private IMAGE_DOS_HEADER ReadDosHeader(BinaryReader r)
        {
            return (IMAGE_DOS_HEADER)ReadRawType(r, typeof(IMAGE_DOS_HEADER));
        }
        private IMAGE_NT_HEADERS ReadNTHeader(BinaryReader r)
        {
            int overallSize = Marshal.SizeOf(typeof(uint)) + Marshal.SizeOf(typeof(IMAGE_FILE_HEADER));
            IMAGE_NT_HEADERS ret = new IMAGE_NT_HEADERS();
            // mapping data to header
            {
                using (BinaryReader headerReader = new BinaryReader(new MemoryStream(r.ReadBytes(overallSize))))
                {
                    headerReader.BaseStream.Position = 0;
                    ret.Signature = headerReader.ReadUInt32();
                    {
                        ret.FileHeader.Machine = headerReader.ReadUInt16();
                        ret.FileHeader.NumberOfSections = headerReader.ReadUInt16();
                        ret.FileHeader.TimeDateStamp = headerReader.ReadUInt32();
                        ret.FileHeader.PointerToSymbolTable = headerReader.ReadUInt32();
                        ret.FileHeader.NumberOfSymbols = headerReader.ReadUInt32();
                        ret.FileHeader.SizeOfOptionalHeader = headerReader.ReadUInt16();
                        ret.FileHeader.SizeOfOptionalHeader = headerReader.ReadUInt16();
                    }
                }
            }

            overallSize += this.NTHeader.FileHeader.SizeOfOptionalHeader;
            int OffsetOfOptionalHeader = Marshal.SizeOf(typeof(uint)) + Marshal.SizeOf(typeof(IMAGE_FILE_HEADER));
            int Size_DATADIRECTORY = 16 * Marshal.SizeOf(typeof(IMAGE_DATA_DIRECTORY));
            int OffsetOfDataDirectory = OffsetOfOptionalHeader + this.NTHeader.FileHeader.SizeOfOptionalHeader - Size_DATADIRECTORY;

            // move to option header
            {
                ret.OptionalHeader.Magic = r.ReadUInt16();
                ret.OptionalHeader.MajorLinkerVersion = r.ReadByte();
                ret.OptionalHeader.MinorLinkerVersion = r.ReadByte();

                ret.OptionalHeader.SizeOfCode = r.ReadUInt32();
                ret.OptionalHeader.SizeOfInitializedData = r.ReadUInt32();
                ret.OptionalHeader.SizeOfUninitializedData = r.ReadUInt32();

                ret.OptionalHeader.AddressOfEntryPoint = r.ReadUInt32();
                ret.OptionalHeader.BaseOfCode = r.ReadUInt32();
                ret.OptionalHeader.BaseOfData = r.ReadUInt32();
                ret.OptionalHeader.ImageBase = r.ReadUInt32();
                ret.OptionalHeader.SectionAlignment = r.ReadUInt32();
                ret.OptionalHeader.FileAlignment = r.ReadUInt32();

                ret.OptionalHeader.MajorOperatingSystemVersion = r.ReadUInt16();
                ret.OptionalHeader.MinorOperatingSystemVersion = r.ReadUInt16();
                ret.OptionalHeader.MajorImageVersion = r.ReadUInt16();
                ret.OptionalHeader.MinorImageVersion = r.ReadUInt16();
                ret.OptionalHeader.MajorSubsystemVersion = r.ReadUInt16();
                ret.OptionalHeader.MinorSubsystemVersion = r.ReadUInt16();

                ret.OptionalHeader.Win32VersionValue = r.ReadUInt32();
                ret.OptionalHeader.SizeOfImage = r.ReadUInt32();
                ret.OptionalHeader.SizeOfHeaders = r.ReadUInt32();
                ret.OptionalHeader.CheckSum = r.ReadUInt32();

                ret.OptionalHeader.Subsystem = r.ReadUInt16();
                ret.OptionalHeader.DllCharacteristics = r.ReadUInt16();

                ret.OptionalHeader.SizeOfStackReserve = r.ReadUInt32();
                ret.OptionalHeader.SizeOfStackCommit = r.ReadUInt32();
                ret.OptionalHeader.SizeOfHeapReserve = r.ReadUInt32();
                ret.OptionalHeader.SizeOfHeapCommit = r.ReadUInt32();
                ret.OptionalHeader.LoaderFlags = r.ReadUInt32();
                ret.OptionalHeader.NumberOfRvaAndSizes = r.ReadUInt32();
            }
            {
                if (ret.OptionalHeader.DataDirectory == null)
                {
                    ret.OptionalHeader.DataDirectory = new IMAGE_DATA_DIRECTORY[16];
                }
                for (int i = 0; i < ret.OptionalHeader.DataDirectory.Length; ++i)
                {  
                    ret.OptionalHeader.DataDirectory[i].VirtualAddress = r.ReadUInt32();
                    ret.OptionalHeader.DataDirectory[i].Size = r.ReadUInt32();
                }
            }

            //return (IMAGE_NT_HEADERS)ReadRawType(r, typeof(IMAGE_NT_HEADERS));
            return ret;
        }

        private bool ReadFromStream(BinaryReader r)
        {
            DosHeader = ReadDosHeader(r);
            if (DosHeader.e_magic != (int)ImageSignatureTypes.IMAGE_DOS_SIGNATURE)
            {
                r.Close();
                return false;
            }
            r.BaseStream.Seek((long)DosHeader.e_lfanew, SeekOrigin.Begin);
            NTHeader = ReadNTHeader(r);
            if (NTHeader.Signature != (int)ImageSignatureTypes.IMAGE_NT_SIGNATURE)
            {
                r.Close();
                return false;
            }
            {
                uint maxpointer = 0;
                uint exesize = 0;
                for (int i = 0; i < NTHeader.FileHeader.NumberOfSections; ++i)
                {
                    IMAGE_SECTION_HEADER data = new IMAGE_SECTION_HEADER();
                    {
                        data.NameBytes = r.ReadBytes(8);
                        data.PhysicalAddress = r.ReadUInt32();
                        data.VirtualAddress = r.ReadUInt32();
                        data.SizeOfRawData = r.ReadUInt32();
                        data.PointerToRawData = r.ReadUInt32();
                        data.PointerToRelocations = r.ReadUInt32();
                        data.PointerToLinenumbers = r.ReadUInt32();
                        data.NumberOfRelocations = r.ReadUInt16();
                        data.NumberOfLinenumbers = r.ReadUInt16();
                        data.Characteristics = r.ReadUInt32();
                    }
                    if (data.PointerToRawData > maxpointer)
                    {
                        maxpointer = data.PointerToRawData;
                        exesize = data.PointerToRawData + data.SizeOfRawData;
                    }
                    SectionHeaders.Add(data);
                }
                this.FileSize = exesize;
            }
            return true;
        }
        private bool Read(String filename, Action<PDBContainer> PDBContainerGetter = null)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (BufferedStream buffered = new BufferedStream(fs))
            using (BinaryReader r = new BinaryReader(buffered))
            {
                bool ret = ReadFromStream(r);
                if (ret && PDBContainerGetter != null)
                {
                    PDBContainer container = PDBContainer.FromBinaryReader(r, this);
                    if (container != null)
                    {
                        PDBContainerGetter(container);
                    }
                }
                return ret;
            }
        }
        private bool ReadFromBytes(byte[] content)
        {
            return this.ReadFromStream(new BinaryReader(new MemoryStream(content)));
        }
        #endregion
        
        List<IMAGE_SECTION_HEADER> SectionHeaders=new List<IMAGE_SECTION_HEADER>();
        public uint FileSize { get; private set; }
        public IMAGE_DOS_HEADER DosHeader { get; private set; }
        public IMAGE_NT_HEADERS NTHeader { get; private set; }
        public static ushort IMAGE_FILE_MACHINE_I386 = 0x014c;
        public static ushort IMAGE_FILE_MACHINE_IA64 = 0x0200;
        public static ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;
        public static ushort IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020;
        
        
        public class PDBContainer
        {
            public ulong ImageBase;
            public uint ImageSize;
            public ulong ImageEntry;
            public String ModuleName;
            public byte[] RawPdb;
            public PEHeaderParser Parser;
            IPDBDataFetcher Fetcher;
            public IPDBDataFetcher GetPdb()
            {
                if(Fetcher == null)
                {
                    String value = Encoding.ASCII.GetString(RawPdb, 0, 4);
                    if(value.Equals("RSDS"))
                    {
                        PDB70Fetcher ret = new PDB70Fetcher() { Parent = Parser };
                        Fetcher = ret;
                    }
                    else if(value.Equals("NB10"))
                    {
                        PDB20Fetcher ret = new PDB20Fetcher() { Parent = Parser };
                        Fetcher = ret;
                    }
                    if(Fetcher!=null)
                        Fetcher.Init(RawPdb);
                }
                return Fetcher;
            }
            private void FillBaseInfo(ulong baseAddr,uint size, ulong entry)
            {
                ImageBase=baseAddr;
                ImageSize = size;
                ImageEntry = entry;
                var pdb = GetPdb();
                if(pdb != null)
                {
                    ModuleName = pdb.GetFileName();
                }
            }
            public PDBContainer(uint size, PEHeaderParser parser)
            {
                this.ImageSize = size;
                this.Parser = parser;
            }
            public static PDBContainer FromBinaryReader(BinaryReader r, PEHeaderParser parser,bool parsingFile=true)
            {
                IMAGE_DATA_DIRECTORY selectDirectory = parser.NTHeader.OptionalHeader.DebuggingInformation;

                
                long addr = 0;
                if (parsingFile)
                {
                    for (int i = 0; i < parser.SectionHeaders.Count; ++i)
                    {
                        var section = parser.SectionHeaders[i];
                        if (selectDirectory.VirtualAddress >= section.VirtualAddress && selectDirectory.VirtualAddress <= section.VirtualAddress + section.VirtualSize)
                        {
                            addr = selectDirectory.VirtualAddress - section.VirtualAddress + section.PointerToRawData;
                        }
                    }
                }
                else
                {
                    addr = selectDirectory.VirtualAddress;
                }
                if (selectDirectory.Size > 0 && addr > 0)
                {
                    r.BaseStream.Position = addr;
                    byte[] buf = r.ReadBytes(unchecked((int)selectDirectory.Size));
                    using (BinaryReader bufFetcher = new BinaryReader(new MemoryStream(buf)))
                    {
                        bufFetcher.BaseStream.Position = 0;
                        DebugDataDirectory debugData = new DebugDataDirectory();
                        debugData.Characteristics = bufFetcher.ReadUInt32();
                        debugData.TimeDateStamp = bufFetcher.ReadUInt32();
                        debugData.MajorVersion = bufFetcher.ReadUInt16();
                        debugData.MinorVersion = bufFetcher.ReadUInt16();

                        debugData.Type = bufFetcher.ReadUInt32();
                        debugData.SizeOfData = bufFetcher.ReadUInt32();
                        debugData.AddressOfRawData = bufFetcher.ReadUInt32();
                        debugData.PointerToRawData = bufFetcher.ReadUInt32();
                        if (debugData.SizeOfData > 0 && debugData.PointerToRawData != 0)
                        {
                            PDBContainer pdbContainer = new PDBContainer(debugData.SizeOfData, parser);
                            r.BaseStream.Position = debugData.PointerToRawData;
                            pdbContainer.RawPdb = r.ReadBytes(unchecked((int)debugData.SizeOfData));
                            pdbContainer.FillBaseInfo(0, parser.FileSize, selectDirectory.VirtualAddress);
                            return pdbContainer;
                        }
                    }

                }
                return null;
            }
        }

        
        public static PEHeaderParser FromFile(String filename, Action<PDBContainer> PDBContainerGetter=null)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);
            PEHeaderParser ret = new PEHeaderParser();
            ret.Read(filename, PDBContainerGetter);
            
            return ret;
        }


    }
}
