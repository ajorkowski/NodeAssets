﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NodeAssets.Core.Compilers;

namespace NodeAssets.Core.SourceManager
{
    public sealed class DefaultSourceManager : ISourceManager, IDisposable
    {
        private IPile _pile;
        private ICompilerConfiguration _compilerManager;
        private readonly DirectoryInfo _compilationDirectory;
        private readonly string _compileExtension;
        private readonly bool _minimise;
        private readonly bool _combine;

        public DefaultSourceManager(bool minimise, bool combine, string compileExtension, string compilationDirectory)
        {
            _compilationDirectory = new DirectoryInfo(compilationDirectory);
            if(!_compilationDirectory.Exists)
            {
                _compilationDirectory.Create();
            }

            _compileExtension = compileExtension;
            _minimise = minimise;
            _combine = combine;
        }

        public void SetPileAsSource(IPile pile, ICompilerConfiguration compilerManager)
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

            CompileAll();
        }

        private IPile _dest;
        public IPile FindDestinationPile()
        {
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
                        var filePath = Path.Combine(_compilationDirectory.FullName, pile + _compileExtension);
                        newPile.AddFile(pile, filePath);
                    }
                    else
                    {
                        foreach (var file in _pile.FindFiles(pile))
                        {
                            var dirPath = Path.Combine(_compilationDirectory.FullName, pile);
                            var filePath = Path.Combine(dirPath, Path.GetFileNameWithoutExtension(file.Name) + _compileExtension);

                            newPile.AddFile(pile, filePath);
                        }
                    }
                }

                _dest = newPile;
            }

            return _dest;
        }

        private void PileOnFileUpdated(object sender, FileChangedEvent fileChangedEvent)
        {
            if (_combine)
            {
                CompilePile(fileChangedEvent.Pile);
            }
            else
            {
                CompileFile(fileChangedEvent.Pile, fileChangedEvent.File);
            }
        }

        private void CompileAll()
        {
            foreach (var pile in _pile.FindAllPiles())
            {
                if (_combine)
                {
                    CompilePile(pile);
                }
                else
                {
                    foreach (var file in _pile.FindFiles(pile))
                    {
                        CompileFile(pile, file);
                    }
                }
            }
        }

        private void CompileFile(string pile, FileInfo file)
        {
            // Here we are keeping the files seperate... so just compile the single file
            // However the file will live in a subDirectory
            var dirPath = Path.Combine(_compilationDirectory.FullName, pile);
            var filePath = Path.Combine(dirPath, Path.GetFileNameWithoutExtension(file.Name) + _compileExtension);

            // Compile/minimise and write to file
            CompileFile(file).ContinueWith(task => AttemptWrite(filePath, task.Result));
        }

        private void CompilePile(string pile)
        {
            // Here we are combining all files
            // If one file in a pile changes... all the rest have to too
            var filePath = Path.Combine(_compilationDirectory.FullName, pile + _compileExtension);

            var tasks = _pile.FindFiles(pile).Select(CompileFile).ToArray();

            Task.Factory.ContinueWhenAll(tasks, t =>
            {
                var builder = new StringBuilder();

                foreach (var task in t)
                {
                    builder.Append(task.Result);
                }

                AttemptWrite(filePath, builder.ToString());
            });
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
                    Thread.Sleep(500);
                    numTries++;
                }
            }
        }

        private Task<string> CompileFile(FileInfo info)
        {
            var compiler = _compilerManager.GetCompiler(info.Name);
            if (compiler == null)
            {
                throw new InvalidOperationException("Compiler could not be found for '" + info.Extension + "' type file");
            }

            ICompiler minCompiler = null;
            if(_minimise)
            {
                minCompiler = _compilerManager.GetCompiler(_compileExtension + ".min");
                if (minCompiler == null)
                {
                    throw new InvalidOperationException("Minimising compiler could not be found for '" + _compileExtension + "' type file");
                }
            }

            // First step is grab the file contents, then continue
            return Task.Factory.StartNew(() => info != null && info.Exists ? File.ReadAllText(info.FullName) : null).ContinueWith(task =>
            {
                // Do the initial compile
                var result = string.Empty;
                if(!string.IsNullOrEmpty(task.Result))
                {
                    try
                    {
                        result = compiler.Compile(task.Result, info).Result;
                    }
                    catch
                    {
                        // Eat compiler exceptions
                    }
                }
                return result;
            }).ContinueWith(task =>
            {
                // Do the minimisation if it has been selected
                var result = task.Result;
                if(_minimise && !string.IsNullOrEmpty(result))
                {
                    try
                    {
                        result = minCompiler.Compile(result, null).Result;
                    }
                    catch
                    {
                        // Eat compiler exceptions
                    }
                    
                }

                return result;
            });
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