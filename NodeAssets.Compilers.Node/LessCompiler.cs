using System.IO;
using System.Threading.Tasks;
using NodeAssets.Core.Commands;

namespace NodeAssets.Compilers
{
    public sealed class LessCompiler : ICompiler
    {
        private const string ScriptLocation = "NodeAssets.Compilers.Node.Scripts.less.js";
        private readonly INodeExecutor _executor;
        private readonly string _executeScript;

        public LessCompiler(INodeExecutor executor)
        {
            _executor = executor;
            _executeScript = ScriptFinder.GetScript(ScriptLocation);
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            initial = initial ?? string.Empty;

            var script = _executeScript
                .Replace("{0}", originalFile != null ? originalFile.FullName : string.Empty);

            var command = _executor.ExecuteJsScript(script);
            command.StdIn.Write(initial);
            command.StdIn.Flush();
            command.StdIn.Close();

            return Task.Factory.StartNew(() => _executor.RunCommand(command));
        }
    }
}
