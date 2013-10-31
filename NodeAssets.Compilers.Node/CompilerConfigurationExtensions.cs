using NodeAssets.Compilers;
using NodeAssets.Core;
using NodeAssets.Core.Commands;

namespace NodeAssets
{
    public static class CompilerConfigurationExtensions
    {
        /// <summary>
        /// Default Node compiler configuration, uses nodejs libraries to compile
        /// </summary>
        /// <param name="nodeWorkspacePath">Path to directory where node_modules are installed</param>
        public static ICompilerConfiguration WithDefaultNodeConfiguration(this ICompilerConfiguration compilerConfig, string nodeWorkspacePath, string nodeExePath = null)
        {
            var passthrough = new PassthroughCompiler();
            var executor = new NodeExecutor(nodeWorkspacePath, nodeExePath);

            return compilerConfig
                .CompilerFor(FileExtensions.Coffee, new CoffeeCompiler(executor))
                .CompilerFor(FileExtensions.Js, passthrough)
                .CompilerFor(FileExtensions.Styl, new StylusCompiler(executor, true))
                .CompilerFor(FileExtensions.Less, new LessCompiler(executor))
                .CompilerFor(FileExtensions.Css, passthrough)
                .CompilerFor(FileExtensions.MinJsMin, passthrough) // ignore .min.js files (they are already minified or want to ignore)
                .CompilerFor(FileExtensions.MinCssMin, passthrough) // ignore .min.css files (they are already minified or want to ignore)
                .CompilerFor(FileExtensions.JsMin, new UglifyJSCompiler(executor))
                .CompilerFor(FileExtensions.CssMin, new CssoCompiler(executor));
        }
    }
}
