using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NodeAssets.Core.SourceManager
{
    public sealed class DefaultSourceManager : ISourceManager, IDisposable
    {
        private readonly DirectoryInfo _compilationDirectory;
        private readonly string _compileExtension;
        private readonly bool _combine;
        private readonly ISourceCompiler _compiler;

        private IPile _pile;
        private CompilerConfiguration _compilerManager;

        public DefaultSourceManager(bool combine, string compileExtension, string compilationDirectory, ISourceCompiler compiler)
        {
            _compilationDirectory = new DirectoryInfo(compilationDirectory);
            if(!_compilationDirectory.Exists)
            {
                _compilationDirectory.Create();
            }

            _compileExtension = compileExtension;
            _combine = combine;
            _compiler = compiler;
        }

        public Task SetPileAsSource(IPile pile, CompilerConfiguration compilerManager)
        {
            if (pile == null) { throw new ArgumentNullException("pile"); }
            if (compilerManager == null) { throw new ArgumentNullException("compilerManager"); }

            if(_pile != null)
            {
                throw new InvalidOperationException("Cannot change the pile once set - use another Source Manager!");
            }

            _compilerManager = compilerManager;
            _pile = pile;

            // Make sure compilation global directory exists
            foreach (var innerPile in _pile.FindAllPiles())
            {
                var dirPath = Path.Combine(_compilationDirectory.FullName, innerPile);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }   
            }

            if (_pile.IsWatchingFiles)
            {
                _pile.FileCreated += PileOnFileUpdated;
                _pile.FileDeleted += PileOnFileUpdated;
                _pile.FileUpdated += PileOnFileUpdated;
            }

            return CompileAndSetupWatchers();
        }

        private IPile _dest;
        public IPile FindDestinationPile()
        {
            if (_pile == null)
            {
                throw new InvalidOperationException("You must first set the pile source via SetPileAsSource");
            }

            if (_dest == null)
            {
                var newPile = new Pile(_pile.IsWatchingFiles);

                foreach (var pile in _pile.FindAllPiles())
                {
                    // Get Urls (just copy over)
                    foreach (var uri in _pile.FindUrls(pile))
                    {
                        newPile.AddUrl(pile, uri.ToString());
                    }

                    // Get New Files
                    if (_combine)
                    {
                        // Only add the pile global file if the file actually has anything
                        if (_pile.FindFiles(pile).Any())
                        {
                            var filePath = Path.Combine(_compilationDirectory.FullName, pile + _compileExtension);
                            newPile.AddFile(pile, filePath);
                        }
                    }
                    else
                    {
                        // We have to add the count in this version to avoid clashes with names...
                        int count = 0;
                        foreach (var file in _pile.FindFiles(pile))
                        {
                            var dirPath = Path.Combine(_compilationDirectory.FullName, pile);
                            var filePath = Path.Combine(dirPath, Path.GetFileNameWithoutExtension(file.Name) + count + _compileExtension);

                            newPile.AddFile(pile, filePath);
                            count++;
                        }
                    }
                }

                _dest = newPile;
            }

            return _dest;
        }

        private async void PileOnFileUpdated(object sender, FileChangedEvent fileChangedEvent)
        {
            if (_combine)
            {
                await CompilePile(fileChangedEvent.Pile, false).ConfigureAwait(false);
            }
            else
            {
                int count = _pile.FindFiles(fileChangedEvent.Pile).TakeWhile(file => file.FullName != fileChangedEvent.File.FullName).Count();
                await CompileFile(fileChangedEvent.Pile, fileChangedEvent.File, count, false).ConfigureAwait(false);
            }
        }

        private Task CompileAndSetupWatchers()
        {
            var tasks = new List<Task>();

            foreach (var pile in _pile.FindAllPiles())
            {
                if (_combine)
                {
                    tasks.Add(CompilePile(pile, true));
                }
                else
                {
                    int count = 0;
                    foreach (var file in _pile.FindFiles(pile))
                    {
                        tasks.Add(CompileFile(pile, file, count, true));
                        count++;
                    }
                }
            }

            // We just create a task as a hook so that you can do things when a compile finishes
            return Task.WhenAll(tasks);
        }

        private async Task CompileFile(string pile, FileInfo file, int count, bool setupWatchers)
        {
            // Here we are keeping the files seperate... so just compile the single file
            // However the file will live in a subDirectory
            var dirPath = Path.Combine(_compilationDirectory.FullName, pile);
            var filePath = Path.Combine(dirPath, Path.GetFileNameWithoutExtension(file.Name) + count + _compileExtension);

            // Compile/minimise and write to file
            var fileData = await _compiler.CompileFile(file, _compilerManager).ConfigureAwait(false);

            // If we are watching, watch the additional deps, but fire the event on the original file
            if (setupWatchers && _pile.IsWatchingFiles && fileData.AdditionalDependencies.Any())
            {
                var changedEvent = new FileChangedEvent(pile, file);
                foreach (var dep in fileData.AdditionalDependencies)
                {
                    var info = new FileInfo(dep);
                    if (info.Exists)
                    {
                        var watcher = new FileSystemWatcher
                        {
                            Path = info.DirectoryName,
                            Filter = info.Name,
                            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime
                        };

                        watcher.Created += (sender, args) => PileOnFileUpdated(pile, changedEvent);
                        watcher.Changed += (sender, args) => PileOnFileUpdated(pile, changedEvent);
                        watcher.Renamed += (sender, args) => PileOnFileUpdated(pile, changedEvent);
                        watcher.Deleted += (sender, args) => PileOnFileUpdated(pile, changedEvent);

                        watcher.EnableRaisingEvents = true;
                    }
                }
            }

            AttemptWrite(filePath, fileData.Output);
        }

        private async Task CompilePile(string pile, bool setupWatchers)
        {
            // Here we are combining all files
            // If one file in a pile changes... all the rest have to too
            var filePath = Path.Combine(_compilationDirectory.FullName, pile + _compileExtension);

            var tasks = _pile.FindFiles(pile).Select(async file => Tuple.Create(await _compiler.CompileFile(file, _compilerManager).ConfigureAwait(false), file));

            if (tasks.Any())
            {
                var compiledFiles = await Task.WhenAll(tasks).ConfigureAwait(false);
                var builder = new StringBuilder();

                foreach (var compiledFile in compiledFiles)
                {
                    // Combine all the outputs
                    builder.Append(compiledFile.Item1.Output);

                    // If we are watching, make sure to watch and compile the original file
                    if (setupWatchers && _pile.IsWatchingFiles && compiledFile.Item1.AdditionalDependencies.Any())
                    {
                        var changedEvent = new FileChangedEvent(pile, compiledFile.Item2);
                        foreach (var dep in compiledFile.Item1.AdditionalDependencies)
                        {
                            var info = new FileInfo(dep);
                            if (info.Exists)
                            {
                                var watcher = new FileSystemWatcher
                                {
                                    Path = info.DirectoryName,
                                    Filter = info.Name,
                                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime
                                };

                                watcher.Created += (sender, args) => PileOnFileUpdated(pile, changedEvent);
                                watcher.Changed += (sender, args) => PileOnFileUpdated(pile, changedEvent);
                                watcher.Renamed += (sender, args) => PileOnFileUpdated(pile, changedEvent);
                                watcher.Deleted += (sender, args) => PileOnFileUpdated(pile, changedEvent);

                                watcher.EnableRaisingEvents = true;
                            }
                        }
                    }
                }

                AttemptWrite(filePath, builder.ToString());
            }
        }

        private void AttemptWrite(string path, string text)
        {
            var numTries = 0;

            // This is crap but apparently the only consistent way to wait for a lock on a file
            while (numTries < 10)
            {
                try
                {
                    File.WriteAllText(path, text);
                    numTries = 11;
                }
                catch (IOException)
                {
                    Thread.Sleep(300);
                    numTries++;
                }
            }
        }

        public void Dispose()
        {
            if(_pile != null && _pile.IsWatchingFiles)
            {
                _pile.FileCreated -= PileOnFileUpdated;
                _pile.FileDeleted -= PileOnFileUpdated;
                _pile.FileUpdated -= PileOnFileUpdated;
                _pile = null;
            }
        }
    }
}
