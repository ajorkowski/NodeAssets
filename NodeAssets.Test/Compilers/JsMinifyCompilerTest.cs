using NodeAssets.Compilers;
using NUnit.Framework;
using System.IO;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class JsMinifyCompilerTest
    {
        private JsMinifyCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new JsMinifyCompiler();
        }

        [Test]
        public void Compile_InvalidJsMinifyFile_Exception()
        {
            Assert.ThrowsAsync(typeof(CompileException), async () =>
            {
                await _compiler.Compile(File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/invalidJS.js"), null).ConfigureAwait(false);
            });
        }

        [Test]
        public void Compile_ValidJsMinifyFile_Compiles()
        {
            var js = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/normalJavascript.js");

            var output = _compiler.Compile(js, null).Result.Output;

            Assert.AreEqual("var helloWorld=function(){console.log(\"Hello World\")};helloWorld();", output);
        }

        [Test]
        public void Compile_BigFile_Compiles()
        {
            var js = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/jquery-1.6.4.js");

            var output = _compiler.Compile(js, null).Result.Output;

            Assert.AreNotEqual(string.Empty, output);
        }
    }
}
