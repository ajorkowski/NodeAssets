using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public sealed class SassCompiler : ICompiler
    {
        private const string LibSassDll = "libsass";

        public SassCompiler(string libsassDllDirectory = null)
        {
            // Internally Sass tries to load from the Assembly.Location, but this doesn't seem to work with asp.net
            // Lets try this as a backup
            if (!string.IsNullOrWhiteSpace(libsassDllDirectory))
            {
                try
                {
                    var loadPath = Path.Combine(libsassDllDirectory, (IntPtr.Size == 8 ? @"x64\" : @"x86\") + LibSassDll + ".dll");
                    LoadLibrary(loadPath);
                }
                catch (Exception) { }
            }
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        public Task<CompileResult> Compile(string initial, FileInfo originalFile)
        {
            try
            {
                var additionalFiles = new List<string>();
                var options = new SharpScss.ScssOptions
                {
                    SourceComments = true,
                    GenerateSourceMap = false
                };

                if (originalFile != null)
                {
                    options.InputFile = originalFile.FullName;
                    options.IncludePaths.Add(originalFile.DirectoryName);
                }

                var output = SharpScss.Scss.ConvertToCss(initial, options);
                return Task.FromResult(new CompileResult
                {
                    Output = output.Css,
                    AdditionalDependencies = output.IncludedFiles
                });
            }
            catch (Exception e)
            {
                throw new CompileException(e.Message, e);
            }
        }
    }
}
