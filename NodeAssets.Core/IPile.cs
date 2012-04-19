using System;
using System.Collections.Generic;
using System.IO;

namespace NodeAssets.Core
{
    public interface IPile
    {
        bool IsWatchingFiles { get; }

        event EventHandler<FileChangedEvent> FileUpdated;
        event EventHandler<FileChangedEvent> FileDeleted;
        event EventHandler<FileChangedEvent> FileCreated;

        IPile AddDirectory(string directory, bool recursive);
        IPile AddDirectory(string pile, string directory, bool recursive);

        IPile AddFile(string fileName);
        IPile AddFile(string pile, string fileName);

        IPile AddUrl(string url);
        IPile AddUrl(string pile, string url);

        IEnumerable<string> FindAllPiles();
        IEnumerable<Uri> FindUrls(string pile);
        IEnumerable<FileInfo> FindFiles(string pile);
        string FindFileHash(FileInfo file);
    }
}
