using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.Optimization;

namespace NodeAssets.Core.Compilers
{
    public sealed class CssMinifyCompiler : ICompiler
    {
        private readonly CssMinify _minifier;

        public CssMinifyCompiler()
        {
            _minifier = new CssMinify();
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            return Task.Factory.StartNew(() =>
            {
                var bundle = new BundleResponse()
                {
                    Content = initial
                };

                _minifier.Process(bundle);

                return bundle.Content;
            });
        }
    }
}
