using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
            return Task.Factory.StartNew(() => RunCommand(ExecuteCoffeeScript(coffee)));
        }

        public Task<string> JsScript(string javascript)
        {
            return Task.Factory.StartNew(() => RunCommand(ExecuteJsScript(javascript)));
        }

        public Task<string> File(FileInfo file)
        {
            return Task.Factory.StartNew(() => RunCommand(ExecuteFile(file)));
        }

        public Task<string> JsFile(FileInfo file)
        {
            return Task.Factory.StartNew(() => RunCommand(ExecuteJsFile(file)));
        }

        public Task<string> CoffeeFile(FileInfo file)
        {
            return Task.Factory.StartNew(() => RunCommand(ExecuteCoffeeFile(file)));
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

        public string RunCommand(CommandResult result)
        {
            var stdOut = result.StdOut.ReadToEnd();
            result.RunningTask.Wait();

            if (result.RunningTask.Result != 0)
            {
                throw new COMException("The execution of the command failed: \r\n" + result.StdErr.ReadToEnd(), result.RunningTask.Result);
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

            var task = Task.Factory.StartNew(() =>
            {
                proc.WaitForExit();
                return proc.ExitCode;
            });

            return new CommandResult(task, proc.StandardOutput, proc.StandardError, proc.StandardInput);
        }

        private string EscapeScript(string script)
        {
            return script.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\\\\r", "\\r").Replace("\\\\n", "\\n");
        }
    }
}
