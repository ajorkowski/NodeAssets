using NodeAssets.Core.Commands;
using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Compilers
{
    public sealed class CoffeeCompiler : ICompiler
    {
        private readonly INodeExecutor _executor;

        public CoffeeCompiler(INodeExecutor executor)
        {
            _executor = executor;
        }

        public async Task<CompileResult> Compile(string initial, FileInfo originalFile)
        {
            // Do not use the original file
            var command = _executor.ExecuteCoffeeCommand("-c -e \"" + initial.Replace("\"", "\\\"") + "\"");
            var output = await _executor.RunCommand(command).ConfigureAwait(false);
            return new CompileResult
            {
                Output = output
            };
        }
    }
}
