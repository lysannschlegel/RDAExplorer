using RDAExplorer.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace RDAExplorer
{
    public class RDAWriter
    {
        public string UI_LastMessage = "";
        public RDAFolder Folder;

        public RDAWriter(RDAFolder folder)
        {
            Folder = folder;
        }

        public void Write(string Filename, FileHeader.Version version, bool compress, RDAReader originalReader, BackgroundWorker wrk)
        {
            FileStream fileStream = new FileStream(Filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);

            // we'll write the header at the end, when we know the offset to the first block
            writer.BaseStream.Position = FileHeader.GetSize(version);
            
            // blocks are organized by file type. there is one RDAFolder per block
            List<RDAFolder> blockFolders = RDABlockCreator.GenerateOf(Folder);
            int numBlocks = (int)originalReader.NumSkippedBlocks + blockFolders.Count;
            BlockInfo[] blockInfos = new BlockInfo[numBlocks];
            ulong[] blockInfoOffsets = new ulong[numBlocks];
            int writeBlockIndex = 0;

            // Write blocks skipped when reading. They have to appear at exactly the place where they came
            // from, because the file data offsets are encrypted and can therefore not be changed.
            for (int skippedBlockIndex = 0; skippedBlockIndex < originalReader.NumSkippedBlocks; ++skippedBlockIndex)
            {
                RDASkippedDataSection skippedBlock = originalReader.SkippedDataSections[skippedBlockIndex];

                if (wrk != null)
                {
                    UI_LastMessage = "Writing  Block " + (writeBlockIndex + 1) + "/" + numBlocks + " => ??? files (encrypted)";
                    wrk.ReportProgress((int)((double)writeBlockIndex / numBlocks * 100.0));
                }

                // Skip ahead to the correct position.
                // This will create "holes" in the file if the skipped sections are not contiguous or 
                // don't start at the beginning of the file, but we'll have to live with it to some extent
                // anyway (we won't fit our "own" data in perfectly). And I'm just too afraid to get the
                // bin-packing wrong.
                writer.BaseStream.WriteBytes((skippedBlock.offset - (ulong)writer.BaseStream.Position), 0);
                
                // write the data
                originalReader.CopySkippedDataSextion(skippedBlock.offset, skippedBlock.size, writer.BaseStream);

                // generate the new block info
                BlockInfo blockInfo = skippedBlock.blockInfo.Clone();
                blockInfos[writeBlockIndex] = blockInfo;
                blockInfoOffsets[writeBlockIndex] = (ulong)writer.BaseStream.Position;

                if (writeBlockIndex > 0)
                {
                    blockInfos[writeBlockIndex - 1].nextBlock = blockInfoOffsets[writeBlockIndex];
                }

                // we'll write the block info at the end, once we know the next block offset
                writer.BaseStream.Position += BlockInfo.GetSize(version);
                ++writeBlockIndex;
            }

            // write regular blocks
            for (int blockFolderIndex = 0; blockFolderIndex < blockFolders.Count; ++blockFolderIndex)
            {
                RDAFolder blockFolder = blockFolders[blockFolderIndex];

                bool compressBlock = compress && blockFolder.RDABlockCreator_FileType_IsCompressable.GetValueOrDefault(false);

                if (wrk != null)
                {
                    UI_LastMessage = "Writing Block " + (writeBlockIndex + 1) + "/" + numBlocks + " => " + blockFolder.Files.Count + " files";
                    wrk.ReportProgress((int)((double)writeBlockIndex / numBlocks * 100.0));
                }

                Dictionary<RDAFile, ulong> dirEntryOffsets = new Dictionary<RDAFile, ulong>();
                Dictionary<RDAFile, ulong> dirEntryCompressedSizes = new Dictionary<RDAFile, ulong>();
                foreach (RDAFile file in blockFolder.Files)
                {
                    byte[] dataToWrite = file.GetData();
                    if (compressBlock)
                        dataToWrite = ZLib.ZLib.Compress(dataToWrite);
                    dirEntryOffsets.Add(file, (ulong)writer.BaseStream.Position);
                    dirEntryCompressedSizes.Add(file, (ulong)dataToWrite.Length);
                    writer.Write(dataToWrite);
                }

                int dirEntrySize = (int)DirEntry.GetSize(version);
                int decompressedDirEntriesSize = blockFolder.Files.Count * dirEntrySize;
                byte[] decompressedDirEntries = new byte[decompressedDirEntriesSize];
                for (int dirEntryIndex = 0; dirEntryIndex < blockFolder.Files.Count; ++dirEntryIndex)
                {
                    RDAFile file = blockFolder.Files[dirEntryIndex];
                    DirEntry dirEntry = new DirEntry() {
                        compressed = dirEntryCompressedSizes[file],
                        filesize = file.UncompressedSize,
                        filename = file.FileName,
                        timestamp = file.TimeStamp.ToTimeStamp(),
                        unknown = 0,
                        offset = dirEntryOffsets[file],
                    };
                    byte[] dirEntryBytes = CreateDirEntryBytes(dirEntry, version);
                    Buffer.BlockCopy(dirEntryBytes, 0, decompressedDirEntries, dirEntryIndex * dirEntrySize, dirEntrySize);
                }
                byte[] compressedDirEntries = compressBlock ? ZLib.ZLib.Compress(decompressedDirEntries) : decompressedDirEntries;
                writer.Write(compressedDirEntries);

                BlockInfo blockInfo = new BlockInfo()
                {
                    flags = compressBlock ? 1u : 0u,
                    fileCount = (uint)blockFolder.Files.Count,
                    directorySize = (ulong)compressedDirEntries.Length,
                    decompressedSize = (ulong)decompressedDirEntriesSize,
                    nextBlock = 0, // will set this at the end of the next block
                };
                blockInfos[writeBlockIndex] = blockInfo;
                blockInfoOffsets[writeBlockIndex] = (ulong)writer.BaseStream.Position;

                if (writeBlockIndex > 0)
                {
                    blockInfos[writeBlockIndex - 1].nextBlock = blockInfoOffsets[writeBlockIndex];
                }

                // we'll write the block info at the end, once we know the next block offset
                writer.BaseStream.Position += BlockInfo.GetSize(version);
                ++writeBlockIndex;
            }
            // the last block gets nextBlockOffset after end of file
            blockInfos[blockInfos.Length - 1].nextBlock = blockInfoOffsets[blockInfos.Length - 1] + BlockInfo.GetSize(version);

            // now write all block infos
            for (int index = 0; index < blockInfos.Length; ++index)
                WriteBlockInfo(writer, blockInfoOffsets[index], blockInfos[index], version);

            // now write the header
            FileHeader fileHeader = FileHeader.Create(version);
            fileHeader.firstBlockOffset = blockInfoOffsets[0];
            WriteHeader(writer, 0, fileHeader, version);

            fileStream.Close();
        }

        private static void WriteHeader(BinaryWriter writer, ulong offset, FileHeader fileHeader, FileHeader.Version version)
        {
            writer.BaseStream.Position = (long)offset;

            byte[] magic = FileHeader.GetMagicBytes(fileHeader.version);
            writer.Write(magic);

            writer.Write(fileHeader.unkown);

            FileHeader.WriteUIntVersionAware(writer, fileHeader.firstBlockOffset, version);
        }

        private static void WriteBlockInfo(BinaryWriter writer, ulong offset, BlockInfo blockInfo, FileHeader.Version version)
        {
            writer.BaseStream.Position = (long)offset;

            writer.Write((System.UInt32)blockInfo.flags);
            writer.Write((System.UInt32)blockInfo.fileCount);
            FileHeader.WriteUIntVersionAware(writer, blockInfo.directorySize, version);
            FileHeader.WriteUIntVersionAware(writer, blockInfo.decompressedSize, version);
            FileHeader.WriteUIntVersionAware(writer, blockInfo.nextBlock, version);
        }

        private static byte[] CreateDirEntryBytes(DirEntry dirEntry, FileHeader.Version version)
        {
            byte[] result = new byte[DirEntry.GetSize(version)];
            MemoryStream memoryStream = new MemoryStream(result);
            BinaryWriter writer = new BinaryWriter(memoryStream);

            byte[] filenameBytes = Encoding.Unicode.GetBytes(dirEntry.filename);
            writer.Write(filenameBytes, 0, (int)Math.Min(DirEntry.GetFilenameSize(), filenameBytes.Length));
            writer.BaseStream.Position = DirEntry.GetFilenameSize();

            FileHeader.WriteUIntVersionAware(writer, dirEntry.offset, version);
            FileHeader.WriteUIntVersionAware(writer, dirEntry.compressed, version);
            FileHeader.WriteUIntVersionAware(writer, dirEntry.filesize, version);
            FileHeader.WriteUIntVersionAware(writer, dirEntry.timestamp, version);
            FileHeader.WriteUIntVersionAware(writer, dirEntry.unknown, version);

            return result;
        }
    }
}
