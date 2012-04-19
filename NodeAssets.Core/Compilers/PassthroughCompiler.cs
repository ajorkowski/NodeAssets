using System.Threading.Tasks;

namespace NodeAssets.Core.Compilers
{
    public sealed class PassthroughCompiler : ICompiler
    {
        public Task<string> Compile(string initial)
        {
            return Task.Factory.StartNew(() => initial);
        }
    }
}
