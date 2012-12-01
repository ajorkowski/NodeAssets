using System.IO;
using NodeAssets.Compilers;
using NodeAssets.Core.Commands;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class CssoCompilerTest
    {
        private CssoCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new CssoCompiler(new NodeExecutor("../../Node"));
        }

        [Test]
        public void Compile_ValidCSSFile_Compiles()
        {
            var css = File.ReadAllText("../../Data/exampleCss.css");

            var result = _compiler.Compile(css, null).Result;

            Assert.AreEqual(".test{color:#fff}\n", result);
        }
    }
}
