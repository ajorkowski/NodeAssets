using System;
using System.IO;
using System.Runtime.InteropServices;
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

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            try
            {
                return Task.FromResult(_compiler.Compress(initial));
            }
            catch(Exception e)
            {
                throw new COMException(e.Message);
            }
        }
    }
}
