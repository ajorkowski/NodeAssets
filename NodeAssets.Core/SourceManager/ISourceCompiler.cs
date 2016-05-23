using NodeAssets.Compilers;
using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Core.SourceManager
{
    public interface ISourceCompiler
    {
        Task<CompileResult> CompileFile(FileInfo file, CompilerConfiguration compilerConfig);
    }
}
