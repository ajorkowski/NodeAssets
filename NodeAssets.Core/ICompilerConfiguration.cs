using NodeAssets.Compilers;
using System;

namespace NodeAssets.Core
{
    public interface ICompilerConfiguration
    {
        ICompilerConfiguration CompilerFor(string extension, ICompiler compiler);
        ICompilerConfiguration OnCompilerError(Action<Exception> onErrorFunc);
        ICompilerConfiguration OnCompileMeasurement(Action<CompileMeasurement> onMeasure);
    }

    public class CompileMeasurement
    {
        public string Compiler { get; set; }
        public string File { get; set; }
        public long CompileTimeMilliseconds { get; set; }
    }
}
