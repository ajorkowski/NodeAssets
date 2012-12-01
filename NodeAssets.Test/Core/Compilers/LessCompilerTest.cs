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
            _executor = new NodeExecutor("../../Node");
        }

        [Test]
        [ExpectedException(typeof(COMException))]
        public void Compile_InvalidLessFile_Exception()
        {
            var file = new FileInfo("../../Data/invalidLess.less");
            var styl = File.ReadAllText("../../Data/invalidLess.less");
            var compiler = new LessCompiler(_executor);

            try
            {
                var output = compiler.Compile(styl, file).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        public void Compile_ValidLess_Compiles()
        {
            var file = new FileInfo("../../Data/exampleLess.less");
            var compiler = new LessCompiler(_executor);
            var styl = File.ReadAllText("../../Data/exampleLess.less");

            var output = compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  border-color: #ccc;\n}\n\n", output);
        }
    }
}
