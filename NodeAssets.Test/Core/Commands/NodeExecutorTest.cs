using System;
using System.IO;

using NodeAssets.Core.Commands;
using NUnit.Framework;
using NodeAssets.Compilers;

namespace NodeAssets.Test.Core.Commands
{
    [TestFixture]
    public class NodeExecutorTest
    {
        private NodeExecutor _executor;

        [SetUp]
        public void Init()
        {
            _executor = new NodeExecutor(TestContext.CurrentContext.TestDirectory + "/../../Node", TestContext.CurrentContext.TestDirectory + "/../../Node/node.exe");
        }

        [Test]
        public void ExecuteJsScript_InValid_NonZeroExitCode()
        {
            var process = _executor.ExecuteJsScript("kdjfhdskjhsd");
            process.RunningTask.Wait();

            Assert.AreNotEqual(0, process.RunningTask.Result);
        }

        [Test]
        public void ExecuteJsScript_ValidJs_Executes()
        {
            var process = _executor.ExecuteJsScript("console.log(\"blah\");\r\nconsole.log(\"blah2\");");
            process.RunningTask.Wait();

            var output = process.StdOut.ReadToEnd();

            Assert.AreEqual("blah\nblah2\n", output);
        }

        [Test]
        public void JsScript_InvalidJS_ThrowsException()
        {
            Assert.ThrowsAsync(typeof(CompileException), async () =>
            {
                await _executor.JsScript("kdjfhdskjhsd").ConfigureAwait(false);
            });
        }

        [Test]
        public void JsScript_ValidJs_Executes()
        {
            var output = _executor.JsScript("console.log(\"blah\");\r\nconsole.log(\"blah2\");").Result;

            Assert.AreEqual("blah\nblah2\n", output);
        }

        [Test]
        public void CoffeeScript_IsValid_Executes()
        {
            var output = _executor.CoffeeScript("console.log \r\n\titem: 'blah'\r\n\titem2: \"blah2\"").Result;

            Assert.AreEqual("{ item: 'blah', item2: 'blah2' }\n", output);
        }

        [Test]
        public void CoffeeScript_InvalidCoffee_ThrowsException()
        {
            Assert.ThrowsAsync(typeof(CompileException), async () =>
            {
                await _executor.CoffeeScript("kdjfh dskjh sd").ConfigureAwait(false);
            });
        }

        [Test]
        public void File_FileDoesNotExist_Exception()
        {
            Assert.ThrowsAsync(typeof(ArgumentException), async () =>
            {
                await _executor.File(new FileInfo("someFile.txt")).ConfigureAwait(false);
            });
        }

        [Test]
        public void File_ValidJs_Executes()
        {
            var output = _executor.File(new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/normalJavascript.js")).Result;

            Assert.AreEqual("Hello World\n", output);
        }

        [Test]
        public void File_ValidCoffeeScript_Executes()
        {
            var output = _executor.File(new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleCoffee.coffee")).Result;

            Assert.AreEqual("Hello World!\n", output);
        }
    }
}
