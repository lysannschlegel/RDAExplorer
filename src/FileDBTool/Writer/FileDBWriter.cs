using System.Collections.Generic;
using System.Linq;

using RDAExplorer.FileDBTool.Structs;

namespace RDAExplorer.FileDBTool.Writer
{
    public class FileDBWriter : System.IDisposable
    {
        private readonly BinaryWriter writer;

        public FileDBWriter(System.IO.Stream stream, bool leaveOpen)
        {
            if (!stream.CanWrite) {
                throw new System.ArgumentException("stream must be writable");
            }

            this.writer = new BinaryWriter(stream, System.Text.Encoding.Unicode, leaveOpen);
        }

        public FileDBWriter(System.IO.Stream stream)
        : this(stream, false)
        { }

        public void Dispose()
        {
            writer.Dispose();
        }

        public void WriteFileDB(AnnoRDA.FileSystem fileSystem, IList<string> archiveFiles)
        {
            Tags tags = this.CreateTags();

            this.WriteArchiveMap(this.writer, tags, fileSystem, archiveFiles);
            this.WriteTag(this.writer, tags, "StructureEnd");
            this.WriteTagsSections(this.writer, tags);
        }

        private Tags CreateTags()
        {
            Dictionary<ushort, Tag> structures = new Dictionary<ushort, Tag> {
                [0x0002] = new Tag(0x0002, "ArchiveMap"),
                [0x0003] = new Tag(0x0003, "FileTree"),
                [0x0004] = new Tag(0x0004, "PathMap"),
                [0x0005] = new Tag(0x0005, "FileMap"),
                [0x0006] = new Tag(0x0006, "ArchiveFiles"),
                [0x0007] = new Tag(0x0007, "ResidentBuffers"),
            };
            Dictionary<ushort, Tag> attributes = new Dictionary<ushort, Tag> {
                [0x8001] = new Tag(0x8001, "FileName"),
                [0x8002] = new Tag(0x8002, "ArchiveFileIndex"),
                [0x8003] = new Tag(0x8003, "Position"),
                [0x8004] = new Tag(0x8004, "CompressedSize"),
                [0x8005] = new Tag(0x8005, "UncompressedSize"),
                [0x8006] = new Tag(0x8006, "ModificationTime"),
                [0x8007] = new Tag(0x8007, "Flags"),
                [0x8008] = new Tag(0x8008, "ResidentBufferIndex"),
                [0x8009] = new Tag(0x8009, "LastArchiveFile"),
                [0x800A] = new Tag(0x800A, "Size"),
                [0x800B] = new Tag(0x800B, "Buffer"),
            };
            return new Tags(structures, attributes);
        }

        private void WriteArchiveMap(BinaryWriter writer, Tags tags, AnnoRDA.FileSystem fileSystem, IList<string> archiveFiles)
        {
            this.WriteTag(writer, tags, "ArchiveMap");

            IList<AnnoRDA.BlockContentsSource> residentBuffers = new List<AnnoRDA.BlockContentsSource>();
            this.WriteFileTree(writer, tags, fileSystem, archiveFiles, residentBuffers);
            this.WriteArchiveFiles(writer, tags, archiveFiles);
            this.WriteResidentBuffers(writer, tags, residentBuffers);

            this.WriteTag(writer, tags, "StructureEnd");
        }
        private void WriteFileTree(BinaryWriter writer, Tags tags, AnnoRDA.FileSystem fileSystem, IList<string> archiveFiles, IList<AnnoRDA.BlockContentsSource> residentBuffers)
        {
            this.WriteTag(writer, tags, "FileTree");
            if (fileSystem.Root.Folders.Any()) {
                this.WritePathMap(writer, tags, fileSystem.Root.Folders, "", archiveFiles, residentBuffers);
            }
            if (fileSystem.Root.Files.Any()) {
                this.WriteFileMap(writer, tags, fileSystem.Root.Files, "", archiveFiles, residentBuffers);
            }
            this.WriteTag(writer, tags, "StructureEnd");
        }
        private void WritePathMap(BinaryWriter writer, Tags tags, IEnumerable<AnnoRDA.Folder> folders, string fullPathSoFar, IList<string> archiveFiles, IList<AnnoRDA.BlockContentsSource> residentBuffers)
        {
            this.WriteTag(writer, tags, "PathMap");
            foreach (var folder in folders.OrderBy(folder => folder.Name, new AnnoRDA.Util.NaturalFilenameStringComparer())) {
                this.WriteAttribute(writer, tags, "String", (string)folder.Name);
                this.WriteFolderContents(writer, tags, folder, fullPathSoFar + folder.Name + "/", archiveFiles, residentBuffers);
            }
            this.WriteTag(writer, tags, "StructureEnd");
        }
        private void WriteFolderContents(BinaryWriter writer, Tags tags, AnnoRDA.Folder folder, string fullPathSoFar, IList<string> archiveFiles, IList<AnnoRDA.BlockContentsSource> residentBuffers)
        {
            this.WriteTag(writer, tags, "List");
            if (folder.Folders.Any()) {
                this.WritePathMap(writer, tags, folder.Folders, fullPathSoFar, archiveFiles, residentBuffers);
            }
            if (folder.Files.Any()) {
                this.WriteFileMap(writer, tags, folder.Files, fullPathSoFar, archiveFiles, residentBuffers);
            }
            this.WriteTag(writer, tags, "StructureEnd");
        }
        private void WriteFileMap(BinaryWriter writer, Tags tags, IEnumerable<AnnoRDA.File> files, string fullPathSoFar, IList<string> archiveFiles, IList<AnnoRDA.BlockContentsSource> residentBuffers)
        {
            this.WriteTag(writer, tags, "FileMap");
            foreach (var file in files.OrderBy(file => file.Name, new AnnoRDA.Util.NaturalFilenameStringComparer())) {
                this.WriteAttribute(writer, tags, "String", (string)file.Name);
                this.WriteFileContents(writer, tags, file, fullPathSoFar + file.Name, archiveFiles, residentBuffers);
            }
            this.WriteTag(writer, tags, "StructureEnd");
        }
        private void WriteFileContents(BinaryWriter writer, Tags tags, AnnoRDA.File file, string fullFilePath, IList<string> archiveFiles, IList<AnnoRDA.BlockContentsSource> residentBuffers)
        {
            this.WriteTag(writer, tags, "List");

            this.WriteAttribute(writer, tags, "FileName", (string)fullFilePath);

            int archiveFileIndex = this.GetIndexInList(archiveFiles, file.ContentsSource.BlockContentsSource.ArchiveFilePath);
            if (archiveFileIndex != 0) {
                this.WriteAttribute(writer, tags, "ArchiveFileIndex", (int)archiveFileIndex);
            }

            if (file.ContentsSource.PositionInBlock != 0) {
                this.WriteAttribute(writer, tags, "Position", (long)file.ContentsSource.PositionInBlock);
            }
            if (file.ContentsSource.CompressedSize != 0) {
                this.WriteAttribute(writer, tags, "CompressedSize", (long)file.ContentsSource.CompressedSize);
            }
            if (file.ContentsSource.UncompressedSize != 0) {
                this.WriteAttribute(writer, tags, "UncompressedSize", (long)file.ContentsSource.UncompressedSize);
            }
            if (file.ModificationTimestamp != 0) {
                this.WriteAttribute(writer, tags, "ModificationTime", (long)file.ModificationTimestamp);
            }
            if (file.ContentsSource.BlockContentsSource.Flags.Value != 0) {
                this.WriteAttribute(writer, tags, "Flags", (int)file.ContentsSource.BlockContentsSource.Flags.Value);
            }

            if (file.ContentsSource.BlockContentsSource.Flags.IsMemoryResident) {
                int residentBufferIndex = this.GetIndexInListAddIfMissing(residentBuffers, file.ContentsSource.BlockContentsSource);
                if (residentBufferIndex != 0) {
                    this.WriteAttribute(writer, tags, "ResidentBufferIndex", (int)residentBufferIndex);
                }
            }

            this.WriteTag(writer, tags, "StructureEnd");
        }
        private int GetIndexInList<T>(IList<T> list, T value)
        {
            int result = list.IndexOf(value);
            if (result == -1) {
                throw new System.ArgumentException("value");
            }
            return result;
        }
        private int GetIndexInListAddIfMissing<T>(IList<T> list, T value)
        {
            int result = list.IndexOf(value);
            if (result == -1) {
                result = list.Count;
                list.Add(value);
            }
            return result;
        }
        private void WriteArchiveFiles(BinaryWriter writer, Tags tags, IList<string> archiveFiles)
        {
            this.WriteTag(writer, tags, "ArchiveFiles");

            foreach (string archiveFile in archiveFiles) {
                this.WriteTag(writer, tags, "List");
                this.WriteAttribute(writer, tags, "String", (string)archiveFile);
                this.WriteAttribute(writer, tags, "String", (string)"");
                this.WriteTag(writer, tags, "StructureEnd");
            }

            this.WriteTag(writer, tags, "StructureEnd");

            this.WriteAttribute(writer, tags, "LastArchiveFile", (string)archiveFiles.Last());
        }
        private void WriteResidentBuffers(BinaryWriter writer, Tags tags, IList<AnnoRDA.BlockContentsSource> residentBuffers)
        {
            this.WriteTag(writer, tags, "ResidentBuffers");

            foreach (AnnoRDA.BlockContentsSource residentBuffer in residentBuffers) {
                this.WriteTag(writer, tags, "List");

                this.WriteAttribute(writer, tags, "Size", (int)residentBuffer.CompressedSize);

                using (var stream = residentBuffer.GetRawReadStream()) {
                    using (var reader = new System.IO.BinaryReader(stream)) {
                        if (stream.Length != residentBuffer.CompressedSize) {
                            throw new System.ArgumentException("resident buffer stream length does not match CompresseSize", "residentBuffers");
                        }
                        byte[] bytes = reader.ReadBytes((int)residentBuffer.CompressedSize);
                        this.WriteAttribute(writer, tags, "Buffer", (byte[])bytes);
                    }
                }

                this.WriteTag(writer, tags, "StructureEnd");
            }

            this.WriteTag(writer, tags, "StructureEnd");
        }

        private void WriteAttribute(BinaryWriter writer, Tags tags, string tagName, string value)
        {
            byte[] bytes;
            if (value.Length == 0) {
                bytes = new byte[1] { 0x0 };
            } else {
                bytes = System.Text.Encoding.Unicode.GetBytes(value);
            }
            this.WriteAttribute(writer, tags, tagName, bytes);
        }
        private void WriteAttribute(BinaryWriter writer, Tags tags, string tagName, int value)
        {
            byte[] bytes = System.BitConverter.GetBytes(value);
            this.WriteAttribute(writer, tags, tagName, bytes);
        }
        private void WriteAttribute(BinaryWriter writer, Tags tags, string tagName, long value)
        {
            byte[] bytes = System.BitConverter.GetBytes(value);
            this.WriteAttribute(writer, tags, tagName, bytes);
        }
        private void WriteAttribute(BinaryWriter writer, Tags tags, string tagName, byte[] value)
        {
            Tag tag = this.GetTag(tags, tagName);
            this.WriteTag(writer, tag);
            writer.Write7BitEncodedInt(value.Length);
            writer.Write(value);
        }
        private void WriteAttribute(BinaryWriter writer, Tag tag, byte[] value)
        {
            this.WriteTag(writer, tag);
            writer.Write7BitEncodedInt(value.Length);
            writer.Write(value);
        }

        private void WriteTag(BinaryWriter writer, Tags tags, string tagName)
        {
            Tag tag = this.GetTag(tags, tagName);
            this.WriteTag(writer, tag);
        }
        private Tag GetTag(Tags tags, string tagName)
        {
            Tag tag;
            if (tags.TryGetTagByName(tagName, out tag)) {
                return tag;
            } else {
                throw new System.ArgumentException("tagName");
            }
        }
        private void WriteTag(BinaryWriter writer, Tag tag)
        {
            writer.Write(tag.ID);
        }

        private void WriteTagsSections(BinaryWriter writer, Tags tags)
        {
            uint tagsSectionOffset = (uint)writer.BaseStream.Position;

            ICollection<Tag> customTags = tags.GetAllCustomTags().ToList();
            this.WriteTagsSection(writer, customTags.Where(tag => tag.Type == Tag.TagType.StructureStart));
            this.WriteTagsSection(writer, customTags.Where(tag => tag.Type == Tag.TagType.Attribute));

            writer.Write(tagsSectionOffset);
        }
        private void WriteTagsSection(BinaryWriter writer, IEnumerable<Tag> tags)
        {
            ICollection<Tag> tagCollection = tags.OrderBy(tag => tag.Name).ToList();

            writer.Write7BitEncodedInt(tagCollection.Count);

            foreach (Tag tag in tags.OrderBy(tag => tag.Name)) {
                writer.WriteZeroTerminatedASCIIString(tag.Name);
                writer.Write(tag.ID);
            }
        }
    }
}
