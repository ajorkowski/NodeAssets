using NodeAssets.Compilers;
using System;

namespace NodeAssets.Core
{
    public interface ICompilerConfiguration
    {
        ICompilerConfiguration CompilerFor(string extension, ICompiler compiler);
        ICompilerConfiguration OnCompilerError(Action<Exception> onErrorFunc);
    }
}
