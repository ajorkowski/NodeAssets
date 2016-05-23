using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public interface ICompiler
    {
        Task<CompileResult> Compile(string initial, FileInfo originalFile);
    }

    public class CompileResult
    {
        public string Output { get; set; }
        public List<string> AdditionalDependencies { get; set; }
    }
}
