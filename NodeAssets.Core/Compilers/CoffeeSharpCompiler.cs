using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoffeeSharp;
using Jurassic;

namespace NodeAssets.Core.Compilers
{
    public sealed class CoffeeSharpCompiler : ICompiler
    {
        private readonly CoffeeScriptEngine _compiler;

        public CoffeeSharpCompiler()
        {
            _compiler = new CoffeeScriptEngine();
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    return _compiler.Compile(initial);
                }
                catch (JavaScriptException e)
                {
                    throw new COMException(e.Message);
                }
            });
        }
    }
}
