using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NodeAssets.Core
{
    public class FileChangedEvent : EventArgs
    {
        private readonly string _pile;
        private readonly FileInfo _file;

        public FileChangedEvent(string pile, FileInfo file)
        {
            _pile = pile;
            _file = file;
        }

        public string Pile { get { return _pile; } }
        public FileInfo File { get { return _file; } }
    }

    public class Pile : IPile
    {
        private const string Global = "global";

        private readonly Dictionary<string, List<Uri>> _urls;
        private readonly Dictionary<string, List<FileInfo>> _files;
        private readonly Dictionary<string, string> _fileHashes;
        private readonly bool _watchFiles;

        public Pile(bool watchFiles)
        {
            _watchFiles = watchFiles;
            _files = new Dictionary<string, List<FileInfo>> {{Global, new List<FileInfo>()}};
            _urls = new Dictionary<string, List<Uri>> {{Global, new List<Uri>()}};
            _fileHashes = new Dictionary<string, string>();
        }

        public bool IsWatchingFiles
        {
            get { return _watchFiles; }
        }

        private event EventHandler<FileChangedEvent> _fileUpdated;

        public event EventHandler<FileChangedEvent> FileUpdated
        {
            add { _fileUpdated += value; }
            remove { _fileUpdated -= value; }
        }

        private event EventHandler<FileChangedEvent> _fileDeleted;

        public event EventHandler<FileChangedEvent> FileDeleted
        {
            add { _fileDeleted += value; }
            remove { _fileDeleted -= value; }
        }

        private event EventHandler<FileChangedEvent> _fileCreated;

        public event EventHandler<FileChangedEvent> FileCreated
        {
            add { _fileCreated += value; }
            remove { _fileCreated -= value; }
        }

        protected virtual void OnFileDeleted(string pile, FileInfo info)
        {
            try
            {
                // Ignore if we have already handled this event
                if(_fileHashes.ContainsKey(info.FullName) && _fileHashes[info.FullName] == string.Empty)
                {
                    return;
                }

                _fileHashes[info.FullName] = string.Empty;

                if (_fileDeleted != null)
                {
                    _fileDeleted(this, new FileChangedEvent(pile, info));
                }
            }
            catch
            {
                // Eat errors here
            }
        }

        protected virtual void OnFileUpdated(string pile, FileInfo info)
        {
            try
            {
                // Ignore if we have already handled this event
                var currentLastWriteTime = GetFileHash(info);
                if (_fileHashes.ContainsKey(info.FullName) && _fileHashes[info.FullName] == currentLastWriteTime)
                {
                    return;
                }

                _fileHashes[info.FullName] = currentLastWriteTime;

                if (_fileUpdated != null)
                {
                    _fileUpdated(this, new FileChangedEvent(pile, info));
                }
            }
            catch
            {
                // Eat errors here
            }
        }

        protected virtual void OnFileCreated(string pile, FileInfo info)
        {
            try
            {
                // Ignore if we have already handled this event
                var currentLastWriteTime = GetFileHash(info);
                if (_fileHashes.ContainsKey(info.FullName) && _fileHashes[info.FullName] == currentLastWriteTime)
                {
                    return;
                }

                _fileHashes[info.FullName] = currentLastWriteTime;

                if (_fileCreated != null)
                {
                    _fileCreated(this, new FileChangedEvent(pile, info));
                }
            }
            catch
            {
                // Eat errors here
            }
        }

        public IEnumerable<string> FindAllPiles()
        {
            return _files.Keys.Union(_urls.Keys);
        }

        public IEnumerable<Uri> FindUrls(string pile)
        {
            List<Uri> uri;
            _urls.TryGetValue(pile, out uri);
            uri = uri ?? new List<Uri>();
            return uri;
        }

        public IEnumerable<FileInfo> FindFiles(string pile)
        {
            List<FileInfo> files;
            _files.TryGetValue(pile, out files);
            files = files ?? new List<FileInfo>();
            return files;
        }

        public IPile AddDirectory(string directory, bool recursive)
        {
            AddDirectory(Global, directory, recursive, null);
            return this;
        }

        public IPile AddDirectory(string directory, bool recursive, Regex allowedFilePattern)
        {
            AddDirectory(Global, directory, recursive, allowedFilePattern);
            return this;
        }

        public IPile AddDirectory(string pile, string directory, bool recursive)
        {
            AddDirectory(pile, directory, recursive, null);
            return this;
        }

        public IPile AddDirectory(string pile, string directory, bool recursive, Regex allowedFilePattern)
        {
            if (pile == null)
            {
                throw new ArgumentNullException("pile", "Pile cannot be null");
            }

            var info = new DirectoryInfo(directory);
            if (!info.Exists)
            {
                throw new DirectoryNotFoundException("The directory '" + directory + "' was not found.");
            }

            AddDirectory(pile, info, recursive, allowedFilePattern);

            return this;
        }

        public string FindFileHash(FileInfo file)
        {
            string hash = string.Empty;
            if(_fileHashes.ContainsKey(file.FullName))
            {
                hash = _fileHashes[file.FullName];
            }
            else
            {
                if (File.Exists(file.FullName))
                {
                    hash = _fileHashes[file.FullName] = GetFileHash(file);
                }
            }

            return hash;
        }

        public IPile AddFile(string fileName)
        {
            AddFile(Global, fileName);

            return this;
        }

        public IPile AddFile(string pile, string fileName)
        {
            if(pile == null)
            {
                throw new ArgumentNullException("pile", "Pile cannot be null");
            }

            var info = new FileInfo(fileName);

            AddFile(pile, info);
            return this;
        }

        private void AddFile(string pile, FileInfo info)
        {
            if (!_files.ContainsKey(pile))
            {
                _files.Add(pile, new List<FileInfo>());
            }

            _files[pile].Add(info);

            if(_watchFiles)
            {
                // Make sure the directory exists
                if (!Directory.Exists(info.DirectoryName))
                {
                    Directory.CreateDirectory(info.DirectoryName);
                }

                var watcher = new FileSystemWatcher
                {
                    Path = info.DirectoryName,
                    Filter = info.Name,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime
                };

                watcher.Created += (sender, args) => OnFileCreated(pile, info);
                watcher.Changed += (sender, args) => OnFileUpdated(pile, info);
                watcher.Renamed += (sender, args) => OnFileUpdated(pile, info);
                watcher.Deleted += (sender, args) => OnFileDeleted(pile, info);

                watcher.EnableRaisingEvents = true;
            }
        }

        public IPile AddUrl(string url)
        {
            AddUrl(Global, url);
            return this;
        }

        public IPile AddUrl(string pile, string url)
        {
            if (pile == null)
            {
                throw new ArgumentNullException("pile", "Pile cannot be null");
            }

            Uri uri;
            if(!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                throw new InvalidCastException("The string '" + url + "' is not a valid url");
            }

            if (!_urls.ContainsKey(pile))
            {
                _urls.Add(pile, new List<Uri>());
            }

            _urls[pile].Add(uri);
            return this;
        }

        private string GetFileHash(FileInfo info)
        {
            return File.GetLastWriteTimeUtc(info.FullName).Ticks.ToString(CultureInfo.InvariantCulture);
        }

        private void AddDirectory(string pile, DirectoryInfo info, bool recursive, Regex filePattern)
        {
            foreach (var fileInfo in info.EnumerateFiles())
            {
                if (filePattern == null || filePattern.IsMatch(fileInfo.FullName))
                {
                    AddFile(pile, fileInfo);
                }
            }

            if (recursive)
            {
                foreach (var directoryInfo in info.EnumerateDirectories())
                {
                    AddDirectory(pile, directoryInfo, true, filePattern);
                }
            }
        }
    }
}
