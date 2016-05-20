using System.IO;
using System.Threading.Tasks;
using NodeAssets.Core.Commands;

namespace NodeAssets.Compilers
{
    public sealed class CoffeeCompiler : ICompiler
    {
        private readonly INodeExecutor _executor;

        public CoffeeCompiler(INodeExecutor executor)
        {
            _executor = executor;
        }

        public Task<string> Compile(string initial, FileInfo originalFile)
        {
            // Do not use the original file
            var command = _executor.ExecuteCoffeeCommand("-c -e \"" + initial.Replace("\"", "\\\"") + "\"");
            return _executor.RunCommand(command);
        }
    }
}
