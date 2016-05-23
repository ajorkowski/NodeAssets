using NodeAssets.Compilers;
using NUnit.Framework;
using System.IO;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class SassCompilerTest
    {
        private SassCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new SassCompiler();
        }

        [Test]
        public void Compile_InvalidSassFile_Exception()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/invalidSass.scss");
            var sass = File.ReadAllText(file.FullName);

            Assert.ThrowsAsync(typeof(CompileException), async () =>
            {
                await _compiler.Compile(sass, file).ConfigureAwait(false);
            });
        }

        [Test]
        public void Compile_ValidSass_Compiles()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleSass.scss");
            var filePath = file.FullName.Replace("\\", "/");
            var sass = File.ReadAllText(file.FullName);

            var result = _compiler.Compile(sass, file).Result;

            Assert.AreEqual("/* line 1, " + filePath + " */\n.base {\n  border-color: #ccc; }\n", result.Output);
            Assert.AreEqual(null, result.AdditionalDependencies);
        }

        [Test]
        public void Compile_ValidSassWithImport_Compiles()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleSassWithImport.scss");
            var filePath = file.FullName.Replace("\\", "/");
            var styl = File.ReadAllText(file.FullName);

            var result = _compiler.Compile(styl, file).Result;

            Assert.AreEqual("/* line 5, " + filePath + " */\n.base {\n  border-color: #ccc;\n  width: auto; }\n", result.Output);
            Assert.AreEqual(1, result.AdditionalDependencies.Count);

            var depFile = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleSassImport.scss");
            var depFilePath = depFile.FullName.Replace("\\", "/");
            Assert.AreEqual(depFilePath, result.AdditionalDependencies[0]);
        }
    }
}
