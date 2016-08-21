using AnnoModificationManager4.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace RDAExplorer
{
    public class RDAFolder
    {
        public FileHeader.Version Version = FileHeader.Version.Invalid;
        public string FullPath = "";
        public string Name = "";
        public List<RDAFile> Files = new List<RDAFile>();
        public List<RDAFolder> Folders = new List<RDAFolder>();
        public bool? RDABlockCreator_FileType_IsCompressable = new bool?();
        public RDAFolder Parent = null;

        public RDAFolder(FileHeader.Version version)
        {
            this.Version = version;
        }
        public RDAFolder(RDAFolder parent)
        {
            this.Parent = parent;
            this.Version = parent.Version;
        }

        public List<RDAFile> GetAllFiles()
        {
            List<RDAFile> list = new List<RDAFile>();
            list.AddRange(Files);
            foreach (RDAFolder rdaFolder in this.Folders)
                list.AddRange(rdaFolder.GetAllFiles());
            return list;
        }

        public List<RDAFolder> GetAllFolders()
        {
            List<RDAFolder> list = new List<RDAFolder>();
            list.AddRange(Folders);
            foreach (RDAFolder rdaFolder in this.Folders)
                list.AddRange(rdaFolder.GetAllFolders());
            return list;
        }

        public List<string> GetAllExtensions()
        {
            List<string> list = new List<string>();
            foreach (RDAFile rdaFile in this.Files)
            {
                string extension = Path.GetExtension(rdaFile.FileName);
                if (!list.Contains(extension))
                    list.Add(extension);
            }
            foreach (RDAFolder rdaFolder in this.Folders)
                list.AddRange(rdaFolder.GetAllExtensions());
            return Enumerable.ToList(Enumerable.Distinct(list));
        }

        public void AddFiles(List<RDAFile> files)
        {
            foreach (RDAFile rdaFile in files)
            {
                if (rdaFile.FileName.Contains("/"))
                    NavigateTo(GetRoot(), Path.GetDirectoryName(rdaFile.FileName), "").Files.Add(rdaFile);
                else
                    Files.Add(rdaFile);
            }
        }

        public RDAFolder GetRoot()
        {
            if (FullPath == "")
                return this;
            return this.Parent.GetRoot();
        }

        public static RDAFolder GenerateFrom(List<RDAFile> file, FileHeader.Version version)
        {
            RDAFolder root = new RDAFolder(version);
            root.Files.AddRange(file.FindAll(f => !f.FileName.Contains("/")));
            foreach (RDAFile rdaFile in file.FindAll(f => f.FileName.Contains("/")))
                NavigateTo(root, Path.GetDirectoryName(rdaFile.FileName), "").Files.Add(rdaFile);
            return root;
        }

        private static RDAFolder NavigateTo(RDAFolder root, string FullPath, string CurrentPos)
        {
            FullPath = FullPath.Replace("\\", "/");
            CurrentPos = CurrentPos.Replace("\\", "/");
            FullPath = FullPath.Trim('/');
            CurrentPos = CurrentPos.Trim('/');
            List<string> list = Enumerable.ToList(FullPath.Split('/'));
            string str = list[0];
            RDAFolder root1 = null;
            foreach (RDAFolder rdaFolder in root.Folders)
            {
                if (rdaFolder.Name == str)
                {
                    root1 = rdaFolder;
                    break;
                }
            }
            if (root1 == null)
            {
                RDAFolder rdaFolder = new RDAFolder(root);
                rdaFolder.Name = str;
                rdaFolder.FullPath = CurrentPos + "/" + str;
                root.Folders.Add(rdaFolder);
                root1 = rdaFolder;
            }
            if (list.Count == 1)
                return root1;
            list.RemoveAt(0);
            return NavigateTo(root1, StringExtension.PutTogether(list, '/'), CurrentPos + "/" + str);
        }
    }
}
