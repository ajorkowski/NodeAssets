using System.Collections.Generic;
using NodeAssets.Core.Commands;
using NodeAssets.Core.Compilers;

namespace NodeAssets.Core
{
    public sealed class CompilerConfiguration : ICompilerConfiguration
    {
        private Dictionary<string, ICompiler> _compilers;

        public CompilerConfiguration()
        {
            _compilers = new Dictionary<string, ICompiler>();
        }

        public ICompilerConfiguration WithDefaultConfiguration(string nodeWorkspacePath)
        {
            var passthrough = new PassthroughCompiler();
            var executor = new NodeExecutor(nodeWorkspacePath);

            // File compilers
            _compilers = new Dictionary<string, ICompiler>();
            _compilers.Add(".coffee", new CoffeeCompiler(executor));
            _compilers.Add(".js", passthrough);
            _compilers.Add(".styl", new StylusCompiler(executor, true));
            _compilers.Add(".css", passthrough);

            // Minification providers
            _compilers.Add(".js.min", new UglifyJSCompiler(executor));
            _compilers.Add(".css.min", new CssoCompiler(executor));

            return this;
        }

        public ICompilerConfiguration CompilerFor(string extension, ICompiler compiler)
        {
            _compilers[extension] = compiler;

            return this;
        }

        public ICompiler GetCompiler(string extension)
        {
            ICompiler result = null;
            _compilers.TryGetValue(extension, out result);
            return result;
        }
    }
}
