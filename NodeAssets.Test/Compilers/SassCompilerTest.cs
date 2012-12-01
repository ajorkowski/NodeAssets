using System;
using System.IO;
using System.Runtime.InteropServices;
using NodeAssets.Compilers;
using NUnit.Framework;

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
        [ExpectedException(typeof(COMException))]
        public void Compile_InvalidSassFile_Exception()
        {
            var file = new FileInfo("../../Data/invalidSass.sass");
            var sass = File.ReadAllText(file.FullName);

            try
            {
                var output = _compiler.Compile(sass, file).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        public void Compile_ValidSass_Compiles()
        {
            var file = new FileInfo("../../Data/exampleSass.sass");
            var sass = File.ReadAllText(file.FullName);

            var output = _compiler.Compile(sass, file).Result;

            Assert.AreEqual(".base {\n  border-color: #cccccc; }\n", output);
        }

        [Test]
        public void Compile_ValidStylusWithImport_Compiles()
        {
            var file = new FileInfo("../../Data/exampleSassWithImport.sass");
            var styl = File.ReadAllText(file.FullName);

            var output = _compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  border-color: #cccccc;\n  width: auto; }\n", output);
        }
    }
}
