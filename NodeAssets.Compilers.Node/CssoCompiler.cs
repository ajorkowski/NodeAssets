﻿using System.IO;
using System.Threading.Tasks;
using NodeAssets.Core.Commands;

namespace NodeAssets.Compilers
{
    public sealed class CssoCompiler : ICompiler
    {
        private const string ScriptLocation = "NodeAssets.Compilers.Node.Scripts.csso.js";
        private readonly INodeExecutor _executor;
        private readonly string _executeScript;

        public CssoCompiler(INodeExecutor executor)
        {
            _executor = executor;

            _executeScript = ScriptFinder.GetScript(ScriptLocation);
        }

        public async Task<CompileResult> Compile(string initial, FileInfo originalFile)
        {
            initial = initial ?? string.Empty;

            var command = _executor.ExecuteJsScript(_executeScript);
            command.StdIn.Write(initial);
            command.StdIn.Flush();
            command.StdIn.Close();

            var output = await _executor.RunCommand(command).ConfigureAwait(false);
            return new CompileResult
            {
                Output = output
            };
        }
    }
}
