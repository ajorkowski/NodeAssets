using System.IO;
using System.Threading.Tasks;
using NodeAssets.Core.Commands;

namespace NodeAssets.Compilers
{
    public sealed class StylusCompiler : ICompiler
    {
        private const string ScriptLocation = "NodeAssets.Compilers.Node.Scripts.stylus.js";
        private readonly INodeExecutor _executor;
        private readonly string _executeScript;
        private readonly bool _useNib;

        public StylusCompiler(INodeExecutor executor, bool useNib)
        {
            _executor = executor;

            _useNib = useNib;
            _executeScript = ScriptFinder.GetScript(ScriptLocation);
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            initial = initial ?? string.Empty;

            var script = _executeScript
                .Replace("{0}", _useNib ? "true" : "false")
                .Replace("{1}", originalFile != null ? originalFile.FullName : string.Empty);

            var command = _executor.ExecuteJsScript(script);
            command.StdIn.Write(initial);
            command.StdIn.Flush();
            command.StdIn.Close();

            return _executor.RunCommand(command);
        }
    }
}
