using NodeAssets.Core;
using NodeAssets.Core.Commands;
using NodeAssets.Core.Compilers;

namespace NodeAssets
{
    public static class CompilerConfigurationExtensions
    {
        /// <summary>
        /// Default configuration for compilers, using only .NET compilers
        /// Has Coffee, Sass/Scss, JsMinify and CssMinify
        /// </summary>
        public static ICompilerConfiguration WithDefaultConfiguration(this ICompilerConfiguration compilerConfig)
        {
            var passthrough = new PassthroughCompiler();
            var sassCompiler = new SassCompiler();

            return compilerConfig
                .CompilerFor(".coffee", new CoffeeSharpCompiler())
                .CompilerFor(".js", passthrough)
                .CompilerFor(".sass", sassCompiler)
                .CompilerFor(".scss", sassCompiler)
                .CompilerFor(".css", passthrough)
                .CompilerFor(".min.js.min", passthrough) // ignore .min.js files (they are already minified or want to ignore)
                .CompilerFor(".min.css.min", passthrough) // ignore .min.css files (they are already minified or want to ignore)
                .CompilerFor(".js.min", new JsMinifyCompiler())
                .CompilerFor(".css.min", new CssMinifyCompiler());
        }

        /// <summary>
        /// Default Node compiler configuration, uses nodejs libraries to compile
        /// </summary>
        /// <param name="nodeWorkspacePath">Path to directory where node_modules are installed</param>
        public static ICompilerConfiguration WithDefaultNodeConfiguration(this ICompilerConfiguration compilerConfig, string nodeWorkspacePath)
        {
            var passthrough = new PassthroughCompiler();
            var executor = new NodeExecutor(nodeWorkspacePath);

            return compilerConfig
                .CompilerFor(".coffee", new CoffeeCompiler(executor))
                .CompilerFor(".js", passthrough)
                .CompilerFor(".styl", new StylusCompiler(executor, true))
                .CompilerFor(".css", passthrough)
                .CompilerFor(".min.js.min", passthrough) // ignore .min.js files (they are already minified or want to ignore)
                .CompilerFor(".min.css.min", passthrough) // ignore .min.css files (they are already minified or want to ignore)
                .CompilerFor(".js.min", new UglifyJSCompiler(executor))
                .CompilerFor(".css.min", new CssoCompiler(executor));
        }
    }
}
