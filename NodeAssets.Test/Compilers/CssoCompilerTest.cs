using NodeAssets.Compilers;
using NodeAssets.Core.Commands;
using NUnit.Framework;
using System.IO;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class CssoCompilerTest
    {
        private CssoCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new CssoCompiler(new NodeExecutor(TestContext.CurrentContext.TestDirectory + "/../../Node", TestContext.CurrentContext.TestDirectory + "/../../Node/node.exe"));
        }

        [Test]
        public void Compile_ValidCSSFile_Compiles()
        {
            var css = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleCss.css");

            var result = _compiler.Compile(css, null).Result.Output;

            Assert.AreEqual(".test{color:#fff}\n", result);
        }
    }
}
