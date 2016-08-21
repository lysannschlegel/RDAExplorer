using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RDAExplorer
{
    public static class RDAFileExtension
    {
        public static string ExtractAll_LastMessage = "";

        public static void ExtractAll(this List<RDAFile> files, string folder)
        {
            if (UISettings.EnableConsole)
                Console.Clear();
            for (int index = 0; index < files.Count; ++index)
            {
                if (UISettings.EnableConsole)
                    Console.WriteLine(((double)index / files.Count * 100.0) + "%");
                files[index].ExtractToRoot(folder);
            }
        }

        public static void ExtractAll(this List<RDAFile> files, string folder, BackgroundWorker wrk)
        {
            if (UISettings.EnableConsole)
                Console.Clear();
            for (int index = 0; index < files.Count; ++index)
            {
                if (index % UISettings.Progress_UpdateFileCount == 0)
                {
                    if (UISettings.EnableConsole)
                        Console.WriteLine(((double)index / files.Count * 100.0) + "%");
                    else
                        wrk.ReportProgress((int)((double)index / files.Count * 100.0));
                    ExtractAll_LastMessage = (index + 1) + " files written";
                }
                files[index].ExtractToRoot(folder);
            }
        }
    }
}
