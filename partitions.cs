using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace DiskProp
{
    class Program
    {
        static void Main(string[] args)
        {
            string str = "";
            foreach (var item in EnumPartitions())
            {
                str += item.ToString();
            }
            Console.Write(str);
            Console.ReadKey();
            return;
        }

        public const int ERROR_MORE_DATA = 234;
        public const uint IOCTL_DISK_GET_DRIVE_GEOMETRY = 0x70000;
        public const uint IOCTL_DISK_GET_DRIVE_GEOMETRY_EX = 0x700a0;
        public const uint IOCTL_DISK_GET_DRIVE_LAYOUT_EX = 0x70050;
        public const uint IOCTL_DISK_GET_MEDIA_TYPES = 0x00070c00;
        public const uint IOCTL_STORAGE_GET_MEDIA_TYPES_EX = (0x0000002d << 16) | (0x0301 << 2);
        public const uint IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x560000;
        public const int PARTITION_ENTRY_UNUSED = 0;
        public const int PARTITION_STYLE_GPT = 1;
        public const int PARTITION_STYLE_MBR = 0;

        [Flags]
        public enum EFIPartitionAttributes : ulong
        {
            GPT_ATTRIBUTE_PLATFORM_REQUIRED = 0x0000000000000001,
            LegacyBIOSBootable = 0x0000000000000004,
            GPT_BASIC_DATA_ATTRIBUTE_NO_DRIVE_LETTER = 0x8000000000000000,
            GPT_BASIC_DATA_ATTRIBUTE_HIDDEN = 0x4000000000000000,
            GPT_BASIC_DATA_ATTRIBUTE_SHADOW_COPY = 0x2000000000000000,
            GPT_BASIC_DATA_ATTRIBUTE_READ_ONLY = 0x1000000000000000
        }

        [Flags]
        public enum FileSystemFeature : uint
        {
            CasePreservedNames = 2,
            CaseSensitiveSearch = 1,
            DaxVolume = 0x20000000,
            FileCompression = 0x10,
            NamedStreams = 0x40000,
            PersistentACLS = 8,
            ReadOnlyVolume = 0x80000,
            SequentialWriteOnce = 0x100000,
            SupportsEncryption = 0x20000,
            SupportsExtendedAttributes = 0x00800000,
            SupportsHardLinks = 0x00400000,
            SupportsObjectIDs = 0x10000,
            SupportsOpenByFileId = 0x01000000,
            SupportsReparsePoints = 0x80,
            SupportsSparseFiles = 0x40,
            SupportsTransactions = 0x200000,
            SupportsUsnJournal = 0x02000000,
            UnicodeOnDisk = 4,
            VolumeIsCompressed = 0x8000,
            VolumeQuotas = 0x20
        }

        public enum MEDIA_TYPE : uint
        {
            Unknown,
            F5_1Pt2_512,
            F3_1Pt44_512,
            F3_2Pt88_512,
            F3_20Pt8_512,
            F3_720_512,
            F5_360_512,
            F5_320_512,
            F5_320_1024,
            F5_180_512,
            F5_160_512,
            RemovableMedia,
            FixedMedia,
            F3_120M_512,
            F3_640_512,
            F5_640_512,
            F5_720_512,
            F3_1Pt2_512,
            F3_1Pt23_1024,
            F5_1Pt23_1024,
            F3_128Mb_512,
            F3_230Mb_512,
            F8_256_128,
            F3_200Mb_512,
            F3_240M_512,
            F3_32M_512
        }

        public enum PARTITION_STYLE : int
        {
            MasterBootRecord = 0,
            GuidPartitionTable = 1,
            Raw = 2
        }



        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY
        {
            public long Cylinders;
            public MEDIA_TYPE MediaType;
            public int TracksPerCylinder;
            public int SectorsPerTrack;
            public int BytesPerSector;

            public long DiskSize
            {
                get
                {
                    return Cylinders * (long)TracksPerCylinder * (long)SectorsPerTrack * (long)BytesPerSector;
                }
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY_EX
        {
            public DISK_GEOMETRY Geometry;
            public long DiskSize;
            public byte Data;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("MediaType: " + Geometry.MediaType.ToString());
                sb.AppendLine("Disk size: " + Geometry.DiskSize.ToString() + " " + (DiskSize / 1024 / 1024 / 1024).ToString() + " GB");
                sb.AppendLine("Sectors/Track: " + Geometry.SectorsPerTrack.ToString());
                sb.AppendLine("Tracks/cylinder: " + Geometry.TracksPerCylinder.ToString());
                sb.AppendLine("Cylinders: " + Geometry.Cylinders.ToString());
                sb.AppendLine("BytesPerSector: " + Geometry.BytesPerSector.ToString());
                return sb.ToString();
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct DRIVE_LAYOUT_INFORMATION_MBR
        {
            public long Signature;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct DRIVE_LAYOUT_INFORMATION_GPT
        {
            public Guid DiskId;
            public long StartingUsableOffset;
            public long UsableLength;
            public long MaxPartitionCount;
        }

        [Serializable]
        [StructLayout(LayoutKind.Explicit)]
        public struct DRIVE_LAYOUT_INFORMATION_UNION
        {
            [FieldOffset(0)]
            public DRIVE_LAYOUT_INFORMATION_MBR Mbr;

            [FieldOffset(0)]
            public DRIVE_LAYOUT_INFORMATION_GPT Gpt;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct DRIVE_PROPERTIES
        {
            public string VolumeName;
            public string LabelName;
            public uint SerialNumber;
            public string FileSystemName;
            public string MountPoint;
            public DriveType Drivetype;
            public uint DiskNumber;
            public long Offset;
            public long Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PARTITION_INFORMATION_MBR
        {
            public byte PartitionType;
            public bool BootIndicator;
            public bool RecognizedPartition;
            public Int32 HiddenSectors;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct PARTITION_INFORMATION_GPT
        {
            public Guid PartitionType;
            public Guid PartitionId;
            public EFIPartitionAttributes Attributes;
            public fixed byte Data[72];
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PARTITION_INFORMATION_UNION
        {
            [FieldOffset(0)]
            public PARTITION_INFORMATION_MBR Mbr;
            [FieldOffset(0)]
            public PARTITION_INFORMATION_GPT Gpt;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct PARTITION_INFORMATION_EX
        {
            [MarshalAs(UnmanagedType.U4)]
            public PARTITION_STYLE PartitionStyle;
            public long StartingOffset;
            public long PartitionLength;
            public int PartitionNumber;
            public bool RewritePartition;
            public PARTITION_INFORMATION_UNION DriveLayoutInformaiton;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct DRIVE_LAYOUT_INFORMATION_EX
        {
            public PARTITION_STYLE PartitionStyle;
            public int PartitionCount;
            public DRIVE_LAYOUT_INFORMATION_UNION DriveLayoutInformaiton;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 128)]
            public PARTITION_INFORMATION_EX[] PartitionEntry;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_EXTENT
        {
            public UInt32 DiskNumber;
            public Int64 StartingOffset;
            public Int64 ExtentLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VOLUME_DISK_EXTENTS
        {
            public UInt32 NumberOfDiskExtents;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public DISK_EXTENT[] Extents;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            IntPtr templateFile
        );

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            IntPtr lpInBuffer, uint nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize,
            out uint lpBytesReturned, IntPtr lpOverlapped
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr FindFirstVolume([Out] StringBuilder lpszVolumeName, uint cchBufferLength);

        [DllImport("kernel32.dll")]
        static extern bool FindNextVolume(IntPtr hFindVolume, [Out] StringBuilder lpszVolumeName, uint cchBufferLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindVolumeClose(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern DriveType GetDriveType([MarshalAs(UnmanagedType.LPStr)] string lpRootPathName);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool GetVolumeInformation(
            string rootPathName,
            StringBuilder volumeNameBuffer,
            int volumeNameSize,
            out uint volumeSerialNumber,
            out uint maximumComponentLength,
            out FileSystemFeature fileSystemFlags,
            StringBuilder fileSystemNameBuffer,
            int nFileSystemNameSize
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetVolumePathNamesForVolumeName(string lpszVolumeName, StringBuilder lpszVolumePathNames, uint cchBuferLength, ref UInt32 lpcchReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(IntPtr hMem);

        static void VolumeInfo(string volname, Partition prt)
        {

            prt.drvprop.VolumeName = volname;

            StringBuilder volumename = new StringBuilder(261);
            StringBuilder fsname = new StringBuilder(261);
            uint sernum, maxlen;
            FileSystemFeature flags;
            if (!GetVolumeInformation(volname, volumename, volumename.Capacity, out sernum, out maxlen, out flags, fsname, fsname.Capacity)) return;
            string labelstr = volumename.ToString();
            string fsnamestr = fsname.ToString();
            prt.drvprop.LabelName = labelstr;
            prt.drvprop.SerialNumber = sernum;
            prt.drvprop.FileSystemName = fsnamestr;
            StringBuilder mountName = new StringBuilder(261);
            maxlen = 0;
            if (GetVolumePathNamesForVolumeName(volname, mountName, 522, ref maxlen))
            {
                prt.drvprop.MountPoint = mountName.ToString();
            }
            else Console.WriteLine("Hiba: {0}", GetLastError());
            DriveType drvtp = GetDriveType(volname);
            prt.drvprop.Drivetype = drvtp;


            if (volname[volname.Length - 1] == '\\') volname = volname.Remove(volname.Length - 1);
            IntPtr volH = CreateFile(volname, 0, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            if ((int)volH == -1)
            {
                Console.WriteLine("CreateFile() error!!!");
                return;
            }
            VOLUME_DISK_EXTENTS vde;
            Type ts = typeof(VOLUME_DISK_EXTENTS);
            int ss = Marshal.SizeOf(ts);
            IntPtr mptr = Marshal.AllocHGlobal(ss);
            uint bret;
            bool success = DeviceIoControl(volH, IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS, IntPtr.Zero, 0, mptr, (uint)ss, out bret, IntPtr.Zero);
            if (!success) return;
            VOLUME_DISK_EXTENTS Vde = (VOLUME_DISK_EXTENTS)Marshal.PtrToStructure(mptr, ts);
            Marshal.FreeHGlobal(mptr);
            for (int i = 0; i < Vde.NumberOfDiskExtents; ++i)
            {
                prt.drvprop.DiskNumber = Vde.Extents[i].DiskNumber;
                prt.drvprop.Offset = Vde.Extents[i].StartingOffset;
                prt.drvprop.Size = Vde.Extents[i].ExtentLength;
            }
        }

        static bool Get_DISK_GEOMETRY_EX(IntPtr handle, IntPtr structure, uint size)
        {
            uint ior;
            bool ret = DeviceIoControl(handle, IOCTL_DISK_GET_DRIVE_GEOMETRY_EX, IntPtr.Zero, 0, structure, size, out ior, IntPtr.Zero);
            return ret;
        }

        static bool Get_DRIVE_LAYOUT_INFORMATION_EX(IntPtr handle, IntPtr structure, uint size)
        {
            uint ior;
            bool ret = DeviceIoControl(handle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX, IntPtr.Zero, 0, structure, size, out ior, IntPtr.Zero);
            return ret;
        }

        public static bool FindVolume([Out] StringBuilder volName, int diskno, long offs, long len)
        {
            IntPtr vol = FindFirstVolume(volName, 261);
            bool success = vol != (IntPtr)(-1);
            while (success)
            {
                volName.Remove(volName.ToString().Length - 1, 1);
                IntPtr volH = CreateFile(volName.ToString(), 0, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                volName.Append("\\");
                if ((int)volH != -1)
                {
                    Type ts = typeof(VOLUME_DISK_EXTENTS);
                    int ss = Marshal.SizeOf(ts);
                    IntPtr mptr = Marshal.AllocHGlobal(ss);
                    uint ior;
                    if (DeviceIoControl(volH, IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS, IntPtr.Zero, 0, mptr, (uint)ss, out ior, IntPtr.Zero))
                    {
                        VOLUME_DISK_EXTENTS vde = (VOLUME_DISK_EXTENTS)Marshal.PtrToStructure(mptr, ts);
                        for (uint i = 0; i < vde.NumberOfDiskExtents; i++)
                        {
                            if (vde.Extents[i].DiskNumber == diskno && vde.Extents[i].StartingOffset == offs && vde.Extents[i].ExtentLength == len)
                            {
                                Marshal.FreeHGlobal(mptr);
                                CloseHandle(volH);
                                FindVolumeClose(vol);
                                return true;
                            }
                        }
                    }
                    Marshal.FreeHGlobal(mptr);
                    CloseHandle(volH);
                }
                success = FindNextVolume(vol, volName, (uint)261);
            }
            FindVolumeClose(vol);
            return false;
        }


        public static List<PhysicalDrive> EnumPartitions()
        {
            byte[] data = new byte[] { 0x87, 0xC0, 0x68, 0xB6, 0xB7, 0x26, 0x99, 0xC7 };
            uint tempuint = 0xEBD0A0A2;
            ushort tempushort = 0xB9E5;
            Guid PARTITION_BASIC_DATA_GUID = new Guid((int)tempuint, (short)tempushort, (short)0x4433, data);
            Type ts = typeof(DRIVE_LAYOUT_INFORMATION_EX);
            int ss = Marshal.SizeOf(ts);
            IntPtr mptr = Marshal.AllocHGlobal(ss);
            Type ts2 = typeof(DISK_GEOMETRY_EX);
            int ss2 = Marshal.SizeOf(ts2);
            IntPtr mptr2 = Marshal.AllocHGlobal(ss2);
            int disknumber = 0;
            List<PhysicalDrive> PhysicalDrives = new List<PhysicalDrive>();
            string ret = "";
            for (int i = 0; ; ++i)
            {
                string volume = "\\\\.\\PhysicalDrive" + disknumber;
                IntPtr h = CreateFile(volume, 0, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                bool success = (int)h != -1;
                if (!success) break;
                PhysicalDrive phdrv = new PhysicalDrive();
                byte[] bt;
                if (Get_DISK_GEOMETRY_EX(h, mptr2, (uint)ss2))
                {
                    DISK_GEOMETRY_EX driveGeom = (DISK_GEOMETRY_EX)Marshal.PtrToStructure(mptr2, ts2);
                    phdrv.number = disknumber;
                    phdrv.driveGeom = driveGeom;
                }
                if (Get_DRIVE_LAYOUT_INFORMATION_EX(h, mptr, (uint)ss))
                {
                    DRIVE_LAYOUT_INFORMATION_EX partitions = (DRIVE_LAYOUT_INFORMATION_EX)Marshal.PtrToStructure(mptr, ts);
                    phdrv.partitionsprop = partitions;
                    int tmppartindex = 0;
                    for (int iPart = 0; iPart < (int)partitions.PartitionCount; ++iPart)
                    {
                        bool partGood = false;
                        bool iseq = partitions.PartitionEntry[iPart].DriveLayoutInformaiton.Gpt.PartitionType == PARTITION_BASIC_DATA_GUID;
                        long offset2 = partitions.PartitionEntry[iPart].StartingOffset;
                        long size2 = partitions.PartitionEntry[iPart].PartitionLength;
                        if (partitions.PartitionEntry[iPart].PartitionStyle == PARTITION_STYLE.MasterBootRecord && partitions.PartitionEntry[iPart].DriveLayoutInformaiton.Mbr.PartitionType != PARTITION_ENTRY_UNUSED && partitions.PartitionEntry[iPart].DriveLayoutInformaiton.Mbr.RecognizedPartition)
                        {
                            ++tmppartindex;
                            partGood = true;
                        }
                        else if (partitions.PartitionEntry[iPart].PartitionStyle == PARTITION_STYLE.GuidPartitionTable && iseq)
                        {
                            ++tmppartindex;
                            partGood = true;
                        }
                        if (partGood)
                        {
                            StringBuilder volumename = new StringBuilder(261);
                            if (FindVolume(volumename, i, partitions.PartitionEntry[iPart].StartingOffset, partitions.PartitionEntry[iPart].PartitionLength))
                            {
                                Partition prt = new Partition();
                                prt.drvprop.Offset = partitions.PartitionEntry[iPart].StartingOffset;
                                prt.drvprop.Size = partitions.PartitionEntry[iPart].PartitionLength;
                                VolumeInfo(volumename.ToString(), prt);
                                phdrv.partitions.Add(prt);
                            }
                        }
                    }
                }
                ++disknumber;
                CloseHandle(h);
                PhysicalDrives.Add(phdrv);
            }
            Marshal.FreeHGlobal(mptr);
            Marshal.FreeHGlobal(mptr2);
            return PhysicalDrives;
        }

        public class Partition
        {
            public int iPart = 0;
            public DRIVE_PROPERTIES drvprop = new DRIVE_PROPERTIES();
        }

        public class PhysicalDrive
        {
            public int number = 0;
            public DISK_GEOMETRY_EX driveGeom = new DISK_GEOMETRY_EX();
            public DRIVE_LAYOUT_INFORMATION_EX partitionsprop = new DRIVE_LAYOUT_INFORMATION_EX();
            public List<Partition> partitions = new List<Partition>();

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("\\\\.\\PhysicalDrive" + number.ToString()); sb.AppendLine("");
                sb.AppendLine(driveGeom.ToString());
                switch (partitionsprop.PartitionStyle)
                {
                    case PARTITION_STYLE.MasterBootRecord: sb.AppendLine("Partition type MBR"); break;
                    case PARTITION_STYLE.GuidPartitionTable: sb.AppendLine("Partition type GPT"); break;
                    default: sb.AppendLine("Partition type unknown"); break;

                }
                sb.AppendLine("");
                for (int i = 0; i < partitions.Count; ++i)
                {
                    sb.AppendLine("Partition " + (i + 1).ToString() + " offset: " + partitionsprop.PartitionEntry[partitions[i].iPart].StartingOffset.ToString() + " length: " + partitionsprop.PartitionEntry[partitions[i].iPart].PartitionLength.ToString() + " " + (partitionsprop.PartitionEntry[partitions[i].iPart].PartitionLength / 1024 / 1024).ToString() + " MB");

                    if (partitions[i].drvprop.VolumeName != null)
                    {
                        sb.AppendLine("VolumeName: " + partitions[i].drvprop.VolumeName);
                        sb.AppendLine("Label: [" + partitions[i].drvprop.LabelName + "]");
                        sb.AppendLine("SerialNumber: " + partitions[i].drvprop.SerialNumber.ToString());
                        sb.AppendLine("FileSystemName: " + partitions[i].drvprop.FileSystemName);
                        sb.AppendLine("Mount point: " + partitions[i].drvprop.MountPoint);
                        sb.Append("Drive type: ");

                        switch (partitions[i].drvprop.Drivetype)
                        {
                            case DriveType.CDRom:
                                sb.AppendLine("CDRom");
                                break;
                            case DriveType.Fixed:
                                sb.AppendLine("Fixed");
                                break;
                            case DriveType.Network:
                                sb.AppendLine("Network");
                                break;
                            case DriveType.NoRootDirectory:
                                sb.AppendLine("NoRootDirectory");
                                break;
                            case DriveType.Ram:
                                sb.AppendLine("Ram");
                                break;
                            case DriveType.Removable:
                                sb.AppendLine("Removable");
                                break;
                            case DriveType.Unknown:
                                sb.AppendLine("Unknown");
                                break;
                            default:
                                break;
                        }
                        sb.AppendLine("Disk nr.: " + partitions[i].drvprop.DiskNumber.ToString());
                        sb.AppendLine("Offset: " + partitions[i].drvprop.Offset.ToString());
                        sb.AppendLine("Length: " + partitions[i].drvprop.Size.ToString() + " " + (partitions[i].drvprop.Size / 1024 / 1024).ToString() + " MB");
                    }
                    sb.AppendLine("");
                }
                sb.AppendLine("");
                sb.AppendLine("");
                return sb.ToString();
            }
        }
    }
}
