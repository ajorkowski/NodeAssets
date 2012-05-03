using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Web.Optimization;

namespace NodeAssets.Core.Compilers
{
    public sealed class JsMinifyCompiler : ICompiler
    {
        private readonly JsMinify _minify;

        public JsMinifyCompiler()
        {
            _minify = new JsMinify();
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            return Task.Factory.StartNew(() =>
            {
                var bundle = new BundleResponse()
                {
                    Content = initial
                };

                _minify.Process(bundle);

                // We throw an error if it can't be minified - can deal with this later
                if(bundle.Content.StartsWith("/* Minification failed. Returning unminified contents."))
                {
                    var start = bundle.Content.IndexOf("\r\n", StringComparison.Ordinal) + 2;
                    var end = bundle.Content.IndexOf("*/", StringComparison.Ordinal) - 3;
                    var error = bundle.Content.Substring(start, end - start);

                    throw new COMException(error);
                }

                return bundle.Content + ";";
            });
        }
    }
}
