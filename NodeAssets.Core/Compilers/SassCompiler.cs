using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NodeAssets.Core.Compilers
{
    public sealed class SassCompiler : ICompiler
    {
        private readonly SassAndCoffee.Ruby.Sass.SassCompiler _compiler;

        public SassCompiler()
        {
            _compiler = new SassAndCoffee.Ruby.Sass.SassCompiler();
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    return _compiler.Compile(originalFile.FullName, false, null);
                }
                catch (Exception e)
                {
                    throw new COMException(e.Message, e.InnerException);
                }
            });
        }
    }
}
