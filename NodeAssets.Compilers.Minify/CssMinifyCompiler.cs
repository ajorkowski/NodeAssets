using System;
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

        public Task<CompileResult> Compile(string initial, FileInfo originalFile)
        {
            try
            {
                var output = _compiler.Compress(initial);
                return Task.FromResult(new CompileResult { Output = output });
            }
            catch (Exception e)
            {
                throw new CompileException(e.Message, e);
            }
        }
    }
}
