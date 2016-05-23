using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodeAssets.Core.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        public async Task<CompileResult> Compile(string initial, FileInfo originalFile)
        {
            initial = initial ?? string.Empty;

            dynamic options = new JObject();
            options.dumpLineNumbers = "comments";
            if (originalFile != null)
            {
                options.filename = originalFile.FullName.Replace('\\', '/');
            }

            var script = _executeScript
                .Replace("{0}", JsonConvert.SerializeObject(options));

            var command = _executor.ExecuteJsScript(script);
            command.StdIn.Write(initial);
            command.StdIn.Flush();
            command.StdIn.Close();

            var jsonStr = await _executor.RunCommand(command).ConfigureAwait(false);
            dynamic resultData;
            try
            {
                resultData = JsonConvert.DeserializeObject(jsonStr);
            }
            catch (JsonReaderException)
            {
                throw new CompileException(jsonStr);
            }
            catch (Exception e)
            {
                throw new CompileException(e.Message, e);
            }

            var deps = new List<string>();
            if (resultData.imports != null)
            {
                foreach(string i in resultData.imports)
                {
                    deps.Add(i);
                }
            }

            return new CompileResult
            {
                Output = (string)resultData.css,
                AdditionalDependencies = deps
            };
        }
    }
}
