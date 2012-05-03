using System.IO;
using System.Threading.Tasks;
using CoffeeSharp;

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
            return Task.Factory.StartNew(() => _compiler.Compile(initial));
        }
    }
}
