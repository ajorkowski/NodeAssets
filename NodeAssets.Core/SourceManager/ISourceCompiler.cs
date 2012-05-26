using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Core.SourceManager
{
    public interface ISourceCompiler
    {
        Task<string> CompileFile(FileInfo file, ICompilerConfiguration compilerConfig);
    }
}
