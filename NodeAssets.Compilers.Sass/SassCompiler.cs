using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public sealed class SassCompiler : ICompiler
    {
        private readonly NSass.SassCompiler _compiler;

        public SassCompiler()
        {
            _compiler = new NSass.SassCompiler();
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            try
            {
                var compiled = _compiler.Compile(initial, NSass.OutputStyle.Nested, true, new [] { originalFile.DirectoryName });
                return Task.FromResult(compiled);
            }
            catch (Exception e)
            {
                throw new COMException(e.Message, e.InnerException);
            }
        }
    }
}
