using System.IO;
using System.Threading.Tasks;
using Yahoo.Yui.Compressor;

namespace NodeAssets.Compilers
{
    public sealed class CssMinifyCompiler : ICompiler
    {
        private readonly CssCompressor _compiler;

        public CssMinifyCompiler()
        {
            _compiler = new CssCompressor();
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            return Task.FromResult(_compiler.Compress(initial));
        }
    }
}
