using System;
using System.IO;
using System.Threading.Tasks;
using Yahoo.Yui.Compressor;

namespace NodeAssets.Compilers
{
    public sealed class JsMinifyCompiler : ICompiler
    {
        private readonly JavaScriptCompressor _compiler;

        public JsMinifyCompiler()
        {
            _compiler = new JavaScriptCompressor();
        }

        public Task<CompileResult> Compile(string initial, FileInfo originalFile)
        {
            try
            {
                var output = _compiler.Compress(initial);
                return Task.FromResult(new CompileResult { Output = output });
            }
            catch(Exception e)
            {
                throw new CompileException(e.Message, e);
            }
        }
    }
}
