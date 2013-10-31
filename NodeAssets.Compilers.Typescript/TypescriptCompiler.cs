using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public sealed class TypescriptCompiler : ICompiler
    {
        private readonly string _tscPath;
        private const string JsExt = ".js";

        public TypescriptCompiler(string tscPath = null)
        {
            _tscPath = tscPath ?? "tsc.exe";
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            return Task.Factory.StartNew(() =>
            {
                ExecuteTypescript(originalFile.FullName);
                var newFile = originalFile.Name.Replace(originalFile.Extension, JsExt);
                var fullPath = Path.Combine(originalFile.DirectoryName, newFile);
                return File.ReadAllText(fullPath);
            });
        }

        private void ExecuteTypescript(string args)
        {
            // /c directive tells cmd to close when it is done
            var procStartInfo = new ProcessStartInfo()
            {
                FileName = _tscPath,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Now we create a process, assign its ProcessStartInfo and start it
            var proc = new Process { StartInfo = procStartInfo };
            proc.Start();

            var task = Task.Factory.StartNew(() =>
            {
                proc.WaitForExit();
                return proc.ExitCode;
            });

            proc.StandardOutput.ReadToEnd();
            task.Wait();
        }
    }
}
