using System;
using System.IO;

namespace Vex.Library
{
    public interface IFileSystem
    {
        string CurrentDirectory { get; set; }
        ulong LastErrorCode { get; set; }

        Stream OpenFile(string fileName);

        int EnumerateFiles(string pattern, Action<string, int> OnFileFound);
        bool IsValid();
    }
}
