using NodeAssets.Compilers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace NodeAssets.Core.SourceManager
{
    public sealed class DefaultSourceCompiler : ISourceCompiler
    {
        private readonly bool _minimise;
        private readonly string _compileExtension;

        public DefaultSourceCompiler(bool minimise, string compileExtension)
        {
            _minimise = minimise;
            _compileExtension = compileExtension;
        }

        public async Task<CompileResult> CompileFile(FileInfo file, CompilerConfiguration compilerConfig)
        {
            if (file == null) { throw new ArgumentNullException("file"); }
            if (compilerConfig == null) { throw new ArgumentNullException("compilerConfig"); }

            var compiler = compilerConfig.GetCompiler(file.Name);
            if (compiler == null)
            {
                throw new InvalidOperationException("Compiler could not be found for '" + file.Extension + "' type file");
            }

            ICompiler minCompiler = null;
            if (_minimise)
            {
                minCompiler = compilerConfig.GetCompiler(Path.GetFileNameWithoutExtension(file.Name) + _compileExtension + ".min");
                if (minCompiler == null)
                {
                    throw new InvalidOperationException("Minimising compiler could not be found for '" + _compileExtension + "' type file");
                }
            }

            // First step is grab the file contents, then continue
            // Do the initial compile
            var fileData = file.Exists ? AttemptRead(file.FullName) : string.Empty;
            var output = string.Empty;
            var deps = new List<string>();
            bool hasErrored = false;
            if (!string.IsNullOrEmpty(fileData))
            {
                try
                {
                    var compileResult = await compiler.Compile(fileData, file).ConfigureAwait(false);
                    output = compileResult.Output;
                    deps = compileResult.AdditionalDependencies ?? deps;
                }
                catch (Exception e)
                {
                    output = "An error occurred during initial compilation: \r\n" + e.GetBaseException().Message;
                    hasErrored = true;
                    compilerConfig.HasException(e);
                }
            }

            // Do the minimisation if it has been selected
            if (!hasErrored && _minimise && !string.IsNullOrEmpty(output))
            {
                try
                {
                    var minResult = await minCompiler.Compile(output, null).ConfigureAwait(false);
                    output = minResult.Output;
                    if (minResult.AdditionalDependencies != null && minResult.AdditionalDependencies.Any())
                    {
                        foreach(var dep in minResult.AdditionalDependencies)
                        {
                            if (!deps.Contains(dep))
                            {
                                deps.Add(dep);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    output = "An error occurred during minification: \r\n" + e.GetBaseException().Message;
                    compilerConfig.HasException(e);
                }

            }

            return new CompileResult
            {
                Output = output,
                AdditionalDependencies = deps
            };
        }

        private string AttemptRead(string path)
        {
            var numTries = 0;
            string result = null;

            // This is lame but apparently the only consistent way to wait for a lock on a file
            while (numTries < 10)
            {
                try
                {
                    result = File.ReadAllText(path);
                    numTries = 11;
                }
                catch (IOException)
                {
                    Thread.Sleep(300);
                    numTries++;
                }
            }

            return result;
        }
    }
}
