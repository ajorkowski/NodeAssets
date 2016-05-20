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
            _executor = new NodeExecutor(TestContext.CurrentContext.TestDirectory + "/../../Node", TestContext.CurrentContext.TestDirectory + "/../../Node/node.exe");
        }

        [Test]
        public void Compile_InvalidStylusFile_Exception()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/invalidStylus.styl");
            var styl = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/invalidStylus.styl");
            var compiler = new StylusCompiler(_executor, false);

            Assert.ThrowsAsync(typeof(COMException), async () =>
            {
                await compiler.Compile(styl, file).ConfigureAwait(false);
            });
        }

        [Test]
        public void Compile_ValidStylusNoNib_Compiles()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleStylus.styl");
            var compiler = new StylusCompiler(_executor, false);
            var styl = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleStylus.styl");

            var output = compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  border-color: #ccc;\n}\n\n", output);
        }

        [Test]
        public void Compile_ValidStylusWithNib_Compiles()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleStylusWithNib.styl");
            var compiler = new StylusCompiler(_executor, true);
            var styl = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleStylusWithNib.styl");

            var output = compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  zoom: 1;\n}\n.base:before,\n.base:after {\n  content: \"\";\n  display: table;\n}\n.base:after {\n  clear: both;\n}\n\n", output);
        }

        [Test]
        public void Compile_ValidStylusWithImport_Compiles()
        {
            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleStylusWithImport.styl");
            var compiler = new StylusCompiler(_executor, true);
            var styl = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleStylusWithImport.styl");

            var output = compiler.Compile(styl, file).Result;

            Assert.AreEqual(".base {\n  border-color: #ccc;\n  width: auto;\n}\n\n", output);
        }
    }
}
