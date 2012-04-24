using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NodeAssets.Core.Helpers;
using System.Threading;

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

        protected virtual void OnFileDeleted(FileSystemWatcher fsw, string pile, FileInfo info)
        {
            try
            {
                fsw.EnableRaisingEvents = false;
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
            finally
            {
                fsw.EnableRaisingEvents = true;
            }
        }

        protected virtual void OnFileUpdated(FileSystemWatcher fsw, string pile, FileInfo info)
        {
            try
            {
                fsw.EnableRaisingEvents = false;
                var text = AttemptRead(info.FullName);

                if (text != null)
                {
                    _fileHashes[info.FullName] = Hash.GetHash(text, Hash.HashType.SHA1);

                    if (_fileUpdated != null)
                    {
                        _fileUpdated(this, new FileChangedEvent(pile, info));
                    }
                }
            }
            catch
            {
                // Eat errors here
            }
            finally
            {
                fsw.EnableRaisingEvents = true;
            }
        }

        protected virtual void OnFileCreated(FileSystemWatcher fsw, string pile, FileInfo info)
        {
            try
            {
                fsw.EnableRaisingEvents = false;

                var text = AttemptRead(info.FullName);

                if (text != null)
                {
                    _fileHashes[info.FullName] = Hash.GetHash(text, Hash.HashType.SHA1);

                    if (_fileCreated != null)
                    {
                        _fileCreated(this, new FileChangedEvent(pile, info));
                    }
                }
            }
            catch
            {
                // Eat errors here
            }
            finally
            {
                fsw.EnableRaisingEvents = true;
            }
        }

        public IEnumerable<string> FindAllPiles()
        {
            return _files.Keys.Union(_urls.Keys);
        }

        public IEnumerable<Uri> FindUrls(string pile)
        {
            var uri = new List<Uri>();
            _urls.TryGetValue(pile, out uri);
            return uri;
        }

        public IEnumerable<FileInfo> FindFiles(string pile)
        {
            var files = new List<FileInfo>();
            _files.TryGetValue(pile, out files);
            return files;
        }

        public IPile AddDirectory(string directory, bool recursive)
        {
            AddDirectory(Global, directory, recursive);

            return this;
        }

        public IPile AddDirectory(string pile, string directory, bool recursive)
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

            AddDirectory(pile, info, recursive);

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
                    hash = _fileHashes[file.FullName] = Hash.GetHash(File.ReadAllText(file.FullName), Hash.HashType.SHA1);
                }
            }

            return hash;
        }

        private void AddDirectory(string pile, DirectoryInfo info, bool recursive)
        {
            foreach (var fileInfo in info.EnumerateFiles())
            {
                AddFile(pile, fileInfo);
            }

            if(recursive)
            {
                foreach (var directoryInfo in info.EnumerateDirectories())
                {
                    AddDirectory(pile, directoryInfo, true);
                }
            }
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

                var watcher = new FileSystemWatcher();
                watcher.Path = info.DirectoryName;
                watcher.Filter = info.Name;
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                watcher.Created += (sender, args) => OnFileCreated(watcher, pile, info);
                watcher.Changed += (sender, args) => OnFileUpdated(watcher, pile, info);
                watcher.Deleted += (sender, args) => OnFileDeleted(watcher, pile, info);

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

        private string AttemptRead(string path)
        {
            var numTries = 0;
            string result = null;

            // This is crap but apparently the only consistent way to wait for a lock on a file
            while (numTries < 10)
            {
                try
                {
                    result = File.ReadAllText(path);
                    numTries = 11;
                }
                catch (IOException)
                {
                    Thread.Sleep(500);
                    numTries++;
                }
            }

            return result;
        }
    }
}
