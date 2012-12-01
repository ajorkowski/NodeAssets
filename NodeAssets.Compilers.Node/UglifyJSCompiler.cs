using System.IO;
using System.Threading.Tasks;
using NodeAssets.Core.Commands;

namespace NodeAssets.Compilers
{
    public sealed class UglifyJSCompiler : ICompiler
    {
        private const string ScriptLocation = "NodeAssets.Compilers.Node.Scripts.uglify.js";
        private readonly INodeExecutor _executor;
        private readonly string _executeScript;

        public UglifyJSCompiler(INodeExecutor executor)
        {
            _executor = executor;

            _executeScript = ScriptFinder.GetScript(ScriptLocation);
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            initial = initial ?? string.Empty;

            var command = _executor.ExecuteJsScript(_executeScript);
            command.StdIn.Write(initial);
            command.StdIn.Flush();
            command.StdIn.Close();

            return Task.Factory.StartNew(() => _executor.RunCommand(command));
        }
    }
}
