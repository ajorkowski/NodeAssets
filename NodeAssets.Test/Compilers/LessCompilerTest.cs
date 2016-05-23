using NodeAssets.Compilers;
using NodeAssets.Core.Commands;
using NUnit.Framework;
using System.IO;

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

            Assert.ThrowsAsync(typeof(CompileException), async () =>
            {
                await compiler.Compile(styl, file).ConfigureAwait(false);
            });
        }

        [Test]
        public void Compile_ValidLess_Compiles()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleLess.less");
            var filePath = file.FullName.Replace('\\', '/');
            var styl = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleLess.less");

            var compiler = new LessCompiler(_executor);
            var result = compiler.Compile(styl, file).Result;

            Assert.AreEqual("/* line 1, " + filePath + " */\n.base {\n  border-color: #ccc;\n}\n", result.Output);
            Assert.AreEqual(0, result.AdditionalDependencies.Count);
        }

        [Test]
        public void Compile_ValidLessWithImport_Compiles()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleLessWithImport.less");
            var filePath = file.FullName.Replace('\\', '/');
            var styl = File.ReadAllText(file.FullName);

            var compiler = new LessCompiler(_executor);
            var result = compiler.Compile(styl, file).Result;

            var depFile = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleLessImport.less").FullName;

            Assert.AreEqual("/* line 2, " + depFile + " */\n.imported {\n  width: auto;\n}\n/* line 5, " + filePath + " */\n.base {\n  border-color: #ccc;\n  width: auto;\n}\n", result.Output);
            Assert.AreEqual(1, result.AdditionalDependencies.Count);
            Assert.AreEqual(depFile, result.AdditionalDependencies[0]);
        }
    }
}
