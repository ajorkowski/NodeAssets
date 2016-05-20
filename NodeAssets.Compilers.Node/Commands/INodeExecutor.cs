using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Core.Commands
{
    public interface INodeExecutor
    {
        Task<string> CoffeeScript(string coffee);
        Task<string> JsScript(string javascript);

        Task<string> File(FileInfo file);
        Task<string> JsFile(FileInfo file);
        Task<string> CoffeeFile(FileInfo file);

        CommandResult ExecuteCoffeeScript(string coffee);
        CommandResult ExecuteJsScript(string javascript);

        CommandResult ExecuteFile(FileInfo file);
        CommandResult ExecuteJsFile(FileInfo file);
        CommandResult ExecuteCoffeeFile(FileInfo file);

        // For more advanced users
        Task<string> RunCommand(CommandResult result);
        CommandResult ExecuteNodeCommand(string args);
        CommandResult ExecuteCoffeeCommand(string args);
    }
}