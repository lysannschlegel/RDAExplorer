using AnnoModificationManager4.Misc;
using RDAExplorer;
using System;
using System.IO;

namespace RDAExplorerGUI.Misc
{
    public static class RDAFileExtension
    {
        public static RDAFileTreeViewItem ToTreeViewItem(this RDAFile file)
        {
            string str = Path.GetExtension(file.FileName).ToLower();
            string file1 = "pack://application:,,,/Images/Icons/page_white.png";
            if (str == ".xml")
                file1 = "pack://application:,,,/Images/Icons/page_white_code.png";
            else if (str == ".txt" || str == ".ini" || str == ".cfg")
                file1 = "pack://application:,,,/Images/Icons/page_white_text.png";
            else if (str == ".jpg" || str == ".bmp" || (str == ".png" || str == ".dds"))
                file1 = "pack://application:,,,/Images/Icons/page_white_picture.png";
            else if (str == ".mp3" || str == ".wav" || str == ".wma")
                file1 = "pack://application:,,,/Images/Icons/sound.png";
            RDAFileTreeViewItem fileTreeViewItem = new RDAFileTreeViewItem();
            fileTreeViewItem.Header = ControlExtension.BuildImageTextblock(file1, Path.GetFileName(file.FileName));
            fileTreeViewItem.SemanticValue = "<File>";
            fileTreeViewItem.File = file;
            return fileTreeViewItem;
        }

        public static void SetFile(this RDAFile rdafile, string file, bool deleteOldFileInTemp)
        {
            rdafile.SetFile(file);
            if (!deleteOldFileInTemp)
                return;
            if (!File.Exists(DirectoryExtension.GetTempWorkingDirectory() + "\\" + rdafile.FileName))
                return;
            try
            {
                File.Delete(DirectoryExtension.GetTempWorkingDirectory() + "\\" + rdafile.FileName);
            }
            catch (Exception)
            {
            }
        }
    }
}