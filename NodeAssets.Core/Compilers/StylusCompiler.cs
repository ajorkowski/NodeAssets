using System;
using System.IO;
using System.Threading.Tasks;
using NodeAssets.Core.Commands;

namespace NodeAssets.Core.Compilers
{
    public sealed class StylusCompiler : ICompiler
    {
        private const string ScriptLocation = "NodeAssets.Core.Compilers.Scripts.stylus.js";
        private readonly INodeExecutor _executor;
        private readonly string _executeScript;
        private readonly bool _useNib;

        public StylusCompiler(INodeExecutor executor, bool useNib)
        {
            _executor = executor;

            _useNib = useNib;
            _executeScript = ScriptFinder.GetScript(ScriptLocation);
        }

        public Task<string> Compile(string initial)
        {
            initial = initial ?? string.Empty;

            var command = _executor.ExecuteJsScript(_executeScript.Replace("{0}", _useNib ? "true" : "false"));
            command.StdIn.Write(initial);
            command.StdIn.Flush();
            command.StdIn.Close();

            return Task.Factory.StartNew(() => _executor.RunCommand(command));
        }
    }
}
