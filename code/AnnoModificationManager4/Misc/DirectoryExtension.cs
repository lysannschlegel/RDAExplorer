using System;
using System.Diagnostics;
using System.IO;

namespace AnnoModificationManager4.Misc
{
    public class DirectoryExtension
    {
        private static string TempWorkingDirectory = "";

        public static void copyDirectory(string Src, string Dst)
        {
            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                Dst += (string)(object)Path.DirectorySeparatorChar;
            if (!Directory.Exists(Dst))
                Directory.CreateDirectory(Dst);
            foreach (string str in Directory.GetFileSystemEntries(Src))
            {
                if (Directory.Exists(str))
                    DirectoryExtension.copyDirectory(str, Dst + Path.GetFileName(str));
                else
                    File.Copy(str, Dst + Path.GetFileName(str), true);
            }
        }

        public static string GetApplicationFolder()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName).Trim('\\');
        }

        public static string GetAppDataFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Trim('\\');
        }

        public static string GetTempWorkingDirectory()
        {
            if (!string.IsNullOrEmpty(TempWorkingDirectory))
                return TempWorkingDirectory;
            string path = StringExtension.MakeUnique(Path.GetTempPath().Trim('\\') + "\\RDAExplorer\\Instance", "", p => Directory.Exists(p));
            Directory.CreateDirectory(path);
            TempWorkingDirectory = path;
            return path;
        }

        public static void CleanDirectory(string dir)
        {
            string[] directories = Directory.GetDirectories(dir);
            string[] files = Directory.GetFiles(dir);
            foreach (string path in directories)
            {
                try {
                    Directory.Delete(path, true);
                } catch { }
            }
            foreach (string path in files)
            {
                try {
                    File.Delete(path);
                } catch { }
            }
        }
    }
}
