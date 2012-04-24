using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Core.Compilers
{
    public interface ICompiler
    {
        Task<string> Compile(string initial, FileInfo originalFile);
    }
}
