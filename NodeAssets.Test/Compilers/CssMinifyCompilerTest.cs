using System.IO;
using NodeAssets.Compilers;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class CssMinifyCompilerTest
    {
        private CssMinifyCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new CssMinifyCompiler();
        }

        [Test]
        public void Compile_ValidCSSFile_Compiles()
        {
            var css = File.ReadAllText("../../Data/exampleCss.css");

            var result = _compiler.Compile(css, null).Result;

            Assert.AreEqual(".test,.test{color:aqua;color:#fff}", result);
        }
    }
}
