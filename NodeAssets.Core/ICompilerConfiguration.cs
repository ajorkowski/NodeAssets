using NodeAssets.Core.Compilers;

namespace NodeAssets.Core
{
    public interface ICompilerConfiguration
    {
        ICompilerConfiguration WithDefaultConfiguration(string nodeWorkspacePath);
        ICompilerConfiguration CompilerFor(string extension, ICompiler compiler);
        ICompiler GetCompiler(string extension);
    }
}
