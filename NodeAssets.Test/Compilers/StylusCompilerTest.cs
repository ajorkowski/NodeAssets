using System;
using System.IO;
using System.Runtime.InteropServices;
using NodeAssets.Compilers;
using NodeAssets.Core.Commands;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class StylusCompilerTest
    {
        private INodeExecutor _executor;

        [SetUp]
        public void Init()
        {
            _executor = new NodeExecutor("../../Node", "../../Node/node.exe");
        }

        [Test]
        [ExpectedException(typeof(COMException))]
        public void Compile_InvalidStylusFile_Exception()
        {
            var file = new FileInfo("../../Data/invalidStylus.styl");
            var styl = File.ReadAllText("../../Data/invalidStylus.styl");
            var compiler = new StylusCompiler(_executor, false);

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
        public void Compile_ValidStylusNoNib_Compiles()
        {
            var file = new FileInfo("../../Data/exampleStylus.styl");
            var compiler = new StylusCompiler(_executor, false);
            var styl = File.ReadAllText("../../Data/exampleStylus.styl");

            var output = compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  border-color: #ccc;\n}\n\n", output);
        }

        [Test]
        public void Compile_ValidStylusWithNib_Compiles()
        {
            var file = new FileInfo("../../Data/exampleStylusWithNib.styl");
            var compiler = new StylusCompiler(_executor, true);
            var styl = File.ReadAllText("../../Data/exampleStylusWithNib.styl");

            var output = compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  zoom: 1;\n}\n.base:before,\n.base:after {\n  content: \"\";\n  display: table;\n}\n.base:after {\n  clear: both;\n}\n\n", output);
        }

        [Test]
        public void Compile_ValidStylusWithImport_Compiles()
        {
            var file = new FileInfo("../../Data/exampleStylusWithImport.styl");
            var compiler = new StylusCompiler(_executor, true);
            var styl = File.ReadAllText("../../Data/exampleStylusWithImport.styl");

            var output = compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  border-color: #ccc;\n  width: auto;\n}\n\n", output);
        }
    }
}
