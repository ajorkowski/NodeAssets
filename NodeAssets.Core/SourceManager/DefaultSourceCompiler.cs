using System;
using System.IO;
using System.Threading.Tasks;
using NodeAssets.Core.Compilers;

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

        public Task<string> CompileFile(FileInfo file, ICompilerConfiguration compilerConfig)
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
            return Task.Factory.StartNew(() =>
            {
                // Do the initial compile
                var fileData = file.Exists ? File.ReadAllText(file.FullName) : string.Empty;
                var result = string.Empty;
                bool hasErrored = false;
                if (!string.IsNullOrEmpty(fileData))
                {
                    try
                    {
                        result = compiler.Compile(fileData, file).Result;
                    }
                    catch (Exception e)
                    {
                        result = "An error occurred during initial compilation: \r\n" + e.Message;
                        hasErrored = true;
                    }
                }

                // Do the minimisation if it has been selected
                if (!hasErrored && _minimise && !string.IsNullOrEmpty(result))
                {
                    try
                    {
                        result = minCompiler.Compile(result, null).Result;
                    }
                    catch (Exception e)
                    {
                        result = "An error occurred during minification: \r\n" + e.Message;
                    }

                }

                return result;
            });
        }
    }
}
