using RDAExplorer.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RDAExplorer
{
    public class RDAReader : IDisposable
    {
        private List<RDAFile> rdaFileEntries = new List<RDAFile>();
        private List<List<RDAFile>> rdaFileBlocks = new List<List<RDAFile>>();
        public RDAFolder rdaFolder = new RDAFolder(FileHeader.Version.Version_2_2);
        public string FileName;
        private BinaryReader read;
        private FileHeader fileHeader;
        public uint rdaReadBlocks, rdaSkippedBlocks, rdaSkippedFiles;
        public BackgroundWorker backgroundWorker;
        public string backgroundWorkerLastMessage;

        public RDAReader()
        {
        }

        public RDAReader(string file)
        {
            FileName = file;
            ReadRDAFile();
        }

        private void UpdateOutput(string message)
        {
            if (UISettings.EnableConsole)
                Console.WriteLine(message);
            if (backgroundWorker == null)
                return;
            backgroundWorkerLastMessage = message;
            backgroundWorker.ReportProgress((int)((double)read.BaseStream.Position / read.BaseStream.Length * 100.0));
        }

        public void ReadRDAFile()
        {
            read = new BinaryReader(new FileStream(FileName, FileMode.Open));

            byte[] firstTwoBytes = read.ReadBytes(2); read.BaseStream.Position = 0;
            if (firstTwoBytes[0] == 'R' && firstTwoBytes[1] == '\0')
            {
                fileHeader = ReadFileHeader(read, FileHeader.Version.Version_2_0);
            }
            else if (firstTwoBytes[0] == 'R' && firstTwoBytes[1] == 'e')
            {
                fileHeader = ReadFileHeader(read, FileHeader.Version.Version_2_2);
            }
            else
            {
                throw new Exception("Invalid or unsupported RDA file!");
            }

            rdaReadBlocks = 0; rdaSkippedBlocks = 0; rdaSkippedFiles = 0;
            ulong nextBlockOffset = fileHeader.firstBlockOffset;
            while (nextBlockOffset <  (ulong)read.BaseStream.Length)
            {
                nextBlockOffset = ReadBlock(nextBlockOffset);
            }

            rdaFolder = RDAFolder.GenerateFrom(rdaFileEntries, fileHeader.version);
            UpdateOutput("Done. " + rdaFileEntries.Count + " files. " + rdaReadBlocks + " blocks read, " + rdaSkippedBlocks + " encrypted blocks skipped (" + rdaSkippedFiles + " files).");
        }

        private static FileHeader ReadFileHeader(BinaryReader reader, FileHeader.Version expectedVersion)
        {
            Encoding expectedEncoding = FileHeader.GetMagicEncoding(expectedVersion);
            string expectedMagic = FileHeader.GetMagic(expectedVersion);
            int expectedByteCount = expectedEncoding.GetByteCount(expectedMagic);
            byte[] actualBytes = reader.ReadBytes(expectedByteCount);
            string actualMagic = expectedEncoding.GetString(actualBytes);

            if (actualMagic == expectedMagic)
            {
                uint unknownBytes = FileHeader.GetUnknownSize(expectedVersion);
                return new FileHeader
                {
                    magic = actualMagic,
                    version = expectedVersion,
                    unkown = reader.ReadBytes((int)unknownBytes),
                    firstBlockOffset = ReadUIntVersionAware(reader, expectedVersion),
                };
            }
            else
            {
                throw new Exception("Invalid or unsupported RDA file!");
            }
        }

        private static ulong ReadUIntVersionAware(BinaryReader reader, FileHeader.Version version)
        {
            return FileHeader.ReadUIntVersionAware(reader, version);
        }
        private ulong ReadUIntVersionAware(BinaryReader reader)
        {
            return ReadUIntVersionAware(reader, fileHeader.version);
        }

        private static uint GetUIntSizeVersionAware(FileHeader.Version version)
        {
            return FileHeader.GetUIntSize(version);
        }
        private uint GetUIntSizeVersionAware()
        {
            return GetUIntSizeVersionAware(fileHeader.version);
        }

        private ulong ReadBlock(ulong Offset)
        {
            UpdateOutput("----- Reading Block at " + Offset);
            read.BaseStream.Position = (long)Offset;

            BlockInfo blockInfo = new BlockInfo {
                flags = read.ReadUInt32(),
                fileCount = read.ReadUInt32(),
                directorySize = ReadUIntVersionAware(read),
                decompressedSize = ReadUIntVersionAware(read),
                nextBlock = ReadUIntVersionAware(read),
            };

            if ((blockInfo.flags & 8) != 8)
            {
                bool isMemoryResident = false;
                bool isEncrypted = false;
                bool isCompressed = false;
                if ((blockInfo.flags & 4) == 4)
                {
                    UpdateOutput("MemoryResident");
                    isMemoryResident = true;
                }
                if ((blockInfo.flags & 2) == 2)
                {
                    UpdateOutput("Encrypted");
                    isEncrypted = true;
                }
                if ((blockInfo.flags & 1) == 1)
                {
                    UpdateOutput("Compressed");
                    isCompressed = true;
                }
                if (blockInfo.flags == 0)
                    UpdateOutput("No Flags");

                if (isEncrypted && fileHeader.version == FileHeader.Version.Version_2_2)
                {
                    UpdateOutput("Encrypted 2.2 blocks are not yet supported. Skipping (" + blockInfo.fileCount + " files).");
                    rdaSkippedFiles += blockInfo.fileCount;
                    ++rdaSkippedBlocks;
                }
                else
                {
                    read.BaseStream.Position = (long)(Offset - blockInfo.directorySize);
                    if (isMemoryResident)
                        read.BaseStream.Position -= GetUIntSizeVersionAware() * 2;
                    byte[] numArray2 = read.ReadBytes((int)blockInfo.directorySize);
                    if (isEncrypted)
                        numArray2 = BinaryExtension.Decrypt(numArray2);
                    if (isCompressed)
                        numArray2 = ZLib.ZLib.Uncompress(numArray2, (int)blockInfo.decompressedSize);

                    RDAMemoryResidentHelper mrm = null;
                    if (isMemoryResident)
                    {
                        ulong compressedSize = ReadUIntVersionAware(read);
                        ulong uncompressedSize = ReadUIntVersionAware(read);
                        mrm = new RDAMemoryResidentHelper((ulong)read.BaseStream.Position - 8 - blockInfo.directorySize - compressedSize, uncompressedSize, compressedSize, read.BaseStream, blockInfo);
                    }

                    uint dirEntrySize = DirEntry.GetSize(fileHeader.version);
                    if (blockInfo.fileCount * dirEntrySize != blockInfo.decompressedSize)
                        throw new Exception("Unexpected directory entry size or count");

                    List<RDAFile> rdaFileBlock = new List<RDAFile>();
                    rdaFileBlocks.Add(rdaFileBlock);
                    ++rdaReadBlocks;
                    UpdateOutput("-- DirEntries:");
                    ReadDirEntries(numArray2, blockInfo, mrm, rdaFileBlock);
                }
            }

            return blockInfo.nextBlock;
        }

        private void ReadDirEntries(byte[] buffer, BlockInfo block, RDAMemoryResidentHelper mrm, List<RDAFile> rdaFileBlock)
        {
            MemoryStream memoryStream = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(memoryStream);

            for (uint fileId = 0; fileId < block.fileCount; ++fileId)
            {
                byte[] fileNameBytes = reader.ReadBytes((int)DirEntry.GetFilenameSize());
                string fileNameString = Encoding.Unicode.GetString(fileNameBytes).Replace("\0", "");

                DirEntry dirEntry = new DirEntry {
                    filename = fileNameString,
                    offset = ReadUIntVersionAware(reader),
                    compressed = ReadUIntVersionAware(reader),
                    filesize = ReadUIntVersionAware(reader),
                    timestamp = ReadUIntVersionAware(reader),
                    unknown = ReadUIntVersionAware(reader),
                };

                RDAFile rdaFile = RDAFile.FromUnmanaged(dirEntry, block, read, mrm);
                rdaFileEntries.Add(rdaFile);
                rdaFileBlock.Add(rdaFile);
            }
        }

        public void Dispose()
        {
            if (read != null)
                read.Close();
            rdaFileEntries.Clear();
            rdaFolder = null;
            foreach (Stream stream in RDAFileStreamCache.Cache.Values)
                stream.Close();
            RDAFileStreamCache.Cache.Clear();
            GC.Collect();
        }
    }
}
