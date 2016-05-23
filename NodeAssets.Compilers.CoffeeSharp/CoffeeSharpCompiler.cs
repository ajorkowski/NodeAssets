using CoffeeSharp;
using Jurassic;
using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public sealed class CoffeeSharpCompiler : ICompiler
    {
        private readonly CoffeeScriptEngine _compiler;

        public CoffeeSharpCompiler()
        {
            _compiler = new CoffeeScriptEngine();
        }

        public Task<CompileResult> Compile(string initial, FileInfo originalFile)
        {
            try
            {
                var output = _compiler.Compile(initial);
                return Task.FromResult(new CompileResult { Output = output });
            }
            catch (JavaScriptException e)
            {
                throw new CompileException(e.Message, e);
            }
        }
    }
}
