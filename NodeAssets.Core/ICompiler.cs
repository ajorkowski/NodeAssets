using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public interface ICompiler
    {
        Task<string> Compile(string initial, FileInfo originalFile);
    }
}
