using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public sealed class PassthroughCompiler : ICompiler
    {
        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            return Task.FromResult(initial);
        }
    }
}
