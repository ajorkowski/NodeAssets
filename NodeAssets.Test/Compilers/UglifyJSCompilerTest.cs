using System;
using System.IO;

using NodeAssets.Compilers;
using NodeAssets.Core.Commands;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class UglifyJSCompilerTest
    {
        private UglifyJSCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new UglifyJSCompiler(new NodeExecutor(TestContext.CurrentContext.TestDirectory + "/../../Node", TestContext.CurrentContext.TestDirectory + "/../../Node/node.exe"));
        }

        [Test]
        public void Compile_InvalidUglifyJSFile_Exception()
        {
            Assert.ThrowsAsync(typeof(CompileException), async () =>
            {
                await _compiler.Compile(File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/invalidJS.js"), null).ConfigureAwait(false);
            });
        }

        [Test]
        public void Compile_ValidUglifyJSFile_Compiles()
        {
            var js = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/normalJavascript.js");

            var output = _compiler.Compile(js, null).Result.Output;

            Assert.AreEqual("var helloWorld=function(){console.log(\"Hello World\")};helloWorld();\n", output);
        }

        [Test]
        public void Compile_BigFile_Compiles()
        {
            var js = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/jquery-1.6.4.js");

            var output = _compiler.Compile(js, null).Result;

            Assert.AreNotEqual(string.Empty, output);
        }
    }
}
