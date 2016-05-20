using System;
using NUnit.Framework;
using NodeAssets.Compilers;
using NodeAssets.Core.Commands;
using System.IO;
using System.Runtime.InteropServices;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    class LessCompilerTest
    {
        private INodeExecutor _executor;

        [SetUp]
        public void Init()
        {
            _executor = new NodeExecutor(TestContext.CurrentContext.TestDirectory + "/../../Node", TestContext.CurrentContext.TestDirectory + "/../../Node/node.exe");
        }

        [Test]
        public void Compile_InvalidLessFile_Exception()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/invalidLess.less");
            var styl = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/invalidLess.less");
            var compiler = new LessCompiler(_executor);

            Assert.ThrowsAsync(typeof(COMException), async () =>
            {
                await compiler.Compile(styl, file).ConfigureAwait(false);
            });
        }

        [Test]
        public void Compile_ValidLess_Compiles()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleLess.less");
            var compiler = new LessCompiler(_executor);
            var styl = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleLess.less");

            var output = compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  border-color: #ccc;\n}\n\n", output);
        }
    }
}
