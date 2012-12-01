using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NodeAssets.Compilers;
using NodeAssets.Core;
using NodeAssets.Core.SourceManager;
using NSubstitute;
using NUnit.Framework;

namespace NodeAssets.Test.Core.SourceManager
{
    [TestFixture]
    public class DefaultSourceCompilerTest
    {
        [Test]
        public void CompileFile_FileDoesNotExist_ReturnsEmptyString()
        {
            var compiler = new DefaultSourceCompiler(false, ".js");

            var result = compiler.CompileFile(new FileInfo("blah.coffee"), GetNodeDefConfig()).Result;

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CompileFile_NoCompilerForFile_ThrowsException()
        {
            var compiler = new DefaultSourceCompiler(false, ".js");

            compiler.CompileFile(new FileInfo("blah.coffee"), new CompilerConfiguration()).Wait();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CompileFile_NoCompilerForMinification_ThrowsException()
        {
            var compiler = new DefaultSourceCompiler(true, ".js");

            var config = new CompilerConfiguration().CompilerFor(".js", new PassthroughCompiler());

            compiler.CompileFile(new FileInfo("blah.js"), config);
        }

        [Test]
        public void CompileFile_ValidFile_PicksCorrectCompiler()
        {
            var compiler = new DefaultSourceCompiler(false, ".js");

            var file = new FileInfo("../../Data/emptyJS.js");

            var jsCompiler = Substitute.For<ICompiler>();
            jsCompiler.Compile(";", file).Returns(Task.Factory.StartNew(() => ";"));

            var config = new CompilerConfiguration().CompilerFor(".js", jsCompiler).CompilerFor(".other", null);

            var result = compiler.CompileFile(file, config).Result;

            jsCompiler.Received().Compile(";", file);
            Assert.AreEqual(";", result);
        }

        [Test]
        public void CompileFile_ValidFileWithDoesNotCompile_ReturnsErrorInString()
        {
            var compiler = new DefaultSourceCompiler(false, ".js");

            var file = new FileInfo("../../Data/emptyJS.js");

            var jsCompiler = Substitute.For<ICompiler>();
            jsCompiler.Compile(";", file).Returns(info => { throw new COMException("Error"); });

            var config = new CompilerConfiguration().CompilerFor(".js", jsCompiler).CompilerFor(".other", null);

            var result = compiler.CompileFile(file, config).Result;

            jsCompiler.Received().Compile(";", file);
            Assert.AreEqual("An error occurred during initial compilation: \r\nError", result);
        }

        [Test]
        public void CompileFile_ValidFile_MinificationPicksCorrectCompiler()
        {
            var compiler = new DefaultSourceCompiler(true, ".js");

            var file = new FileInfo("../../Data/emptyJS.js");

            var minCompiler = Substitute.For<ICompiler>();
            minCompiler.Compile(";", null).Returns(Task.Factory.StartNew(() => ";"));

            var config = new CompilerConfiguration().CompilerFor(".js", new PassthroughCompiler()).CompilerFor(".js.min", minCompiler);

            var result = compiler.CompileFile(file, config).Result;

            minCompiler.Received().Compile(";", null);
            Assert.AreEqual(";", result);
        }

        [Test]
        public void CompileFile_ValidFileWithDoesNotCompile_MinificationReturnsErrorInString()
        {
            var compiler = new DefaultSourceCompiler(true, ".js");

            var file = new FileInfo("../../Data/emptyJS.js");

            var minCompiler = Substitute.For<ICompiler>();
            minCompiler.Compile(";", null).Returns(info => { throw new COMException("Error"); });

            var config = new CompilerConfiguration().CompilerFor(".js", new PassthroughCompiler()).CompilerFor(".js.min", minCompiler);

            var result = compiler.CompileFile(file, config).Result;

            minCompiler.Received().Compile(";", null);
            Assert.AreEqual("An error occurred during minification: \r\nError", result);
        }

        [Test]
        public void CompileFile_ValidMinJsFile_IgnoresMinificationStep()
        {
            var compiler = new DefaultSourceCompiler(true, ".js");

            var file = new FileInfo("../../Data/emptyJS.min.js");

            var rightCompiler = Substitute.For<ICompiler>();
            rightCompiler.Compile(";", null).Returns(Task.Factory.StartNew(() => "Right"));

            var wrongCompiler = Substitute.For<ICompiler>();
            wrongCompiler.Compile(";", null).Returns(Task.Factory.StartNew(() => "Wrong"));

            var config = new CompilerConfiguration()
                .CompilerFor(".js", new PassthroughCompiler())
                .CompilerFor(".min.js.min", rightCompiler)
                .CompilerFor(".js.min", wrongCompiler);

            var result = compiler.CompileFile(file, config).Result;

            Assert.AreEqual("Right", result);
        }

        private ICompilerConfiguration GetNodeDefConfig()
        {
            return new CompilerConfiguration().WithDefaultNodeConfiguration("Node");
        }
    }
}
