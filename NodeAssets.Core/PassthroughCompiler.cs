using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public sealed class PassthroughCompiler : ICompiler
    {
        public Task<CompileResult> Compile(string initial, FileInfo originalFile)
        {
            return Task.FromResult(new CompileResult { Output = initial });
        }
    }
}
