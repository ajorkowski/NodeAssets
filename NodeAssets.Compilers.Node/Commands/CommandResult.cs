using System;
using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.Core.Commands
{
    public sealed class CommandResult : IDisposable
    {
        public CommandResult(Task<int> runningTask, StreamReader stdOut, StreamReader stdErr, StreamWriter stdIn)
        {
            RunningTask = runningTask;
            StdOut = stdOut;
            StdErr = stdErr;
            StdIn = stdIn;
        }

        public Task<int> RunningTask { get; private set; }
        public StreamReader StdOut { get; private set; }
        public StreamReader StdErr { get; private set; }
        public StreamWriter StdIn { get; private set; }

        public void Dispose()
        {
            RunningTask.Dispose();
        }
    }
}
