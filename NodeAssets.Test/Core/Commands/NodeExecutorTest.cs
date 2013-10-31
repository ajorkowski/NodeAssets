using System;
using System.IO;
using System.Runtime.InteropServices;
using NodeAssets.Core.Commands;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Commands
{
    [TestFixture]
    public class NodeExecutorTest
    {
        private NodeExecutor _executor;

        [SetUp]
        public void Init()
        {
            _executor = new NodeExecutor("../../Node", "../../Node/node.exe");
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
        [ExpectedException(typeof(COMException))]
        public void JsScript_InvalidJS_ThrowsException()
        {
            try
            {
                var task = _executor.JsScript("kdjfhdskjhsd");
                task.Wait();
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
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
        [ExpectedException(typeof(COMException))]
        public void CoffeeScript_InvalidCoffee_ThrowsException()
        {
            try
            {
                var task = _executor.CoffeeScript("kdjfh dskjh sd");
                task.Wait();
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void File_FileDoesNotExist_Exception()
        {
            try
            {
                var task = _executor.File(new FileInfo("someFile.txt"));
                task.Wait();
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        public void File_ValidJs_Executes()
        {
            var output = _executor.File(new FileInfo("../../Data/normalJavascript.js")).Result;

            Assert.AreEqual("Hello World\n", output);
        }

        [Test]
        public void File_ValidCoffeeScript_Executes()
        {
            var output = _executor.File(new FileInfo("../../Data/exampleCoffee.coffee")).Result;

            Assert.AreEqual("Hello World!\n", output);
        }
    }
}
