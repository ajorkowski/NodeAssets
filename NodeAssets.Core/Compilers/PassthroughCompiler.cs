using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Core.Compilers
{
    public sealed class PassthroughCompiler : ICompiler
    {
        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            return Task.Factory.StartNew(() => initial);
        }
    }
}
