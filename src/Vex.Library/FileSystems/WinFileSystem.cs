using System;
using System.Collections.Generic;
using System.IO;

namespace Vex.Library
{
    internal class WinFileSystem : IFileSystem
    {
        public WinFileSystem(string currentDirectory)
        {
            CurrentDirectory = currentDirectory;
            if (!Directory.Exists(currentDirectory))
            {
                LastErrorCode = 0x5005050;
            }
            else
            {
                LastErrorCode = 0;
            }
        }

        public string CurrentDirectory { get; set; }
        public ulong LastErrorCode { get; set; }

        public int EnumerateFiles(string pattern, Action<string, int> OnFileFound)
        {
            int results = 0;
            var p = pattern.Insert(0, "*");
            List<string> dirs = [CurrentDirectory + "//"];
            while (dirs.Count > 0)
            {
                string currPath = dirs[^1];
                dirs.RemoveAt(dirs.Count - 1);

                foreach (var file in Directory.EnumerateFiles(currPath, p, SearchOption.AllDirectories))
                {
                    OnFileFound(file, (int)new FileInfo(file).Length);
                    results++;
                }

                // Locate sub dirs
                foreach (string directoryName in Directory.EnumerateDirectories(currPath))
                {
                    dirs.Add(directoryName + "\\");
                }
            }

            return results;
        }

        public Stream OpenFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                LastErrorCode = 0;
                return File.OpenRead(fileName);
            }
            else
            {
                LastErrorCode = 0x505000;
                return null;
            }
        }

        public bool IsValid() => LastErrorCode == 0;
    }
}
