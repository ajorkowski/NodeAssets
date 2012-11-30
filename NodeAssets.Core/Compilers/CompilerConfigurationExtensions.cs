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
                .CompilerFor(FileExtensions.Coffee, new CoffeeSharpCompiler())
                .CompilerFor(FileExtensions.Js, passthrough)
                .CompilerFor(FileExtensions.Sass, sassCompiler)
                .CompilerFor(FileExtensions.Scss, sassCompiler)
                .CompilerFor(FileExtensions.Css, passthrough)
                .CompilerFor(FileExtensions.MinJsMin, passthrough) // ignore .min.js files (they are already minified or want to ignore)
                .CompilerFor(FileExtensions.MinCssMin, passthrough) // ignore .min.css files (they are already minified or want to ignore)
                .CompilerFor(FileExtensions.JsMin, new JsMinifyCompiler())
                .CompilerFor(FileExtensions.CssMin, new CssMinifyCompiler());
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
