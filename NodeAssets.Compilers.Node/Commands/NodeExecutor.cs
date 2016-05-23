using NodeAssets.Compilers;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Core.Commands
{
    public sealed class NodeExecutor : INodeExecutor
    {
        private const string CoffeeScriptPath = "\\node_modules\\coffee-script\\bin\\coffee";
        private readonly DirectoryInfo _workspace;
        private readonly string _nodeExePath;

        public NodeExecutor(string nodeWorkspace, string nodeExePath = null)
        {
            // Allow for a null workspace
            if (!string.IsNullOrEmpty(nodeWorkspace))
            {
                _workspace = new DirectoryInfo(nodeWorkspace);
            }

            _nodeExePath = nodeExePath ?? "node.exe";
        }

        public Task<string> CoffeeScript(string coffee)
        {
            return RunCommand(ExecuteCoffeeScript(coffee));
        }

        public Task<string> JsScript(string javascript)
        {
            return RunCommand(ExecuteJsScript(javascript));
        }

        public Task<string> File(FileInfo file)
        {
            return RunCommand(ExecuteFile(file));
        }

        public Task<string> JsFile(FileInfo file)
        {
            return RunCommand(ExecuteJsFile(file));
        }

        public Task<string> CoffeeFile(FileInfo file)
        {
            return RunCommand(ExecuteCoffeeFile(file));
        }

        public CommandResult ExecuteCoffeeScript(string coffee)
        {
            if (string.IsNullOrWhiteSpace(coffee))
            {
                throw new ArgumentException("No coffeescript specified to execute");
            }

            return ExecuteCoffeeCommand("-e \"" + EscapeScript(coffee) + "\"");
        }

        public CommandResult ExecuteJsScript(string javascript)
        {
            if (string.IsNullOrWhiteSpace(javascript))
            {
                throw new ArgumentException("No javascript specified to execute");
            }

            return ExecuteNodeCommand("-e \"" + EscapeScript(javascript) + "\"");
        }

        public CommandResult ExecuteFile(FileInfo file)
        {
            if(file == null) { throw new ArgumentNullException("file"); }
            if (!file.Exists) { throw new ArgumentException("File '" + file.Name + "' does not exist"); }

            var extension = file.Extension;
            if(extension != ".coffee" && extension != ".js")
            {
                throw new ArgumentException("The file extension '" + extension + "' is not known. Use .coffee or .js");
            }

            return extension == ".coffee" ? ExecuteCoffeeFile(file) : ExecuteJsFile(file);
        }

        public CommandResult ExecuteJsFile(FileInfo file)
        {
            if (file == null) { throw new ArgumentNullException("file"); }
            if (!file.Exists) { throw new ArgumentException("File '" + file.Name + "' does not exist"); }

            return ExecuteNodeCommand(file.FullName);
        }

        public CommandResult ExecuteCoffeeFile(FileInfo file)
        {
            if (file == null) { throw new ArgumentNullException("file"); }
            if (!file.Exists) { throw new ArgumentException("File '" + file.Name + "' does not exist"); }

            return ExecuteCoffeeCommand(file.FullName);
        }

        public async Task<string> RunCommand(CommandResult result)
        {
            var stdOut = await result.StdOut.ReadToEndAsync().ConfigureAwait(false);
            var code = await result.RunningTask.ConfigureAwait(false);

            if (code != 0)
            {
                var error = await result.StdErr.ReadToEndAsync().ConfigureAwait(false);
                throw new CompileException("The execution of the command failed: \r\n" + error);
            }

            return stdOut;
        }
        
        public CommandResult ExecuteCoffeeCommand(string args)
        {
            if (_workspace == null || !_workspace.Exists)
            {
                throw new InvalidOperationException("Need a working directory so that we can run coffee-script (npm install coffee-script)");
            }

            var coffee = _workspace.FullName + CoffeeScriptPath;
            return ExecuteNodeCommand(coffee + " " + args);
        }

        public CommandResult ExecuteNodeCommand(string args)
        {
            // /c directive tells cmd to close when it is done
            var procStartInfo = new ProcessStartInfo()
            {
                FileName = _nodeExePath,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (_workspace != null && _workspace.Exists)
            {
                procStartInfo.WorkingDirectory = _workspace.FullName;
            }

            // Now we create a process, assign its ProcessStartInfo and start it
            var proc = new Process { StartInfo = procStartInfo };
            proc.Start();
            
            return new CommandResult(WaitForExitAsync(proc), proc.StandardOutput, proc.StandardError, proc.StandardInput);
        }

        private string EscapeScript(string script)
        {
            return script.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\\\\r", "\\r").Replace("\\\\n", "\\n");
        }

        public static Task<int> WaitForExitAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(process.ExitCode);
            return tcs.Task;
        }
    }
}
