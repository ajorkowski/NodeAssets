using System;
using System.IO;

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

            var result = compiler.CompileFile(new FileInfo("blah.coffee"), GetNodeDefConfig()).Result.Output;

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void CompileFile_NoCompilerForFile_ThrowsException()
        {
            var compiler = new DefaultSourceCompiler(false, ".js");

            Assert.ThrowsAsync(typeof(InvalidOperationException), async () =>
            {
                await compiler.CompileFile(new FileInfo("blah.coffee"), new CompilerConfiguration()).ConfigureAwait(false);
            });
        }

        [Test]
        public void CompileFile_NoCompilerForMinification_ThrowsException()
        {
            var compiler = new DefaultSourceCompiler(true, ".js");

            var config = new CompilerConfiguration().CompilerFor(".js", new PassthroughCompiler()) as CompilerConfiguration;

            Assert.ThrowsAsync(typeof(InvalidOperationException), async () =>
            {
                await compiler.CompileFile(new FileInfo("blah.js"), config).ConfigureAwait(false);
            });
        }

        [Test]
        public void CompileFile_ValidFile_PicksCorrectCompiler()
        {
            var compiler = new DefaultSourceCompiler(false, ".js");

            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/emptyJS.js");

            var jsCompiler = Substitute.For<ICompiler>();
            jsCompiler.Compile(";", file).Returns(Task.Factory.StartNew(() => new CompileResult { Output = ";" }));

            var config = new CompilerConfiguration().CompilerFor(".js", jsCompiler).CompilerFor(".other", null) as CompilerConfiguration;

            var result = compiler.CompileFile(file, config).Result.Output;

            jsCompiler.Received().Compile(";", file);
            Assert.AreEqual(";", result);
        }

        [Test]
        public void CompileFile_ValidFileWithDoesNotCompile_ReturnsErrorInString()
        {
            var compiler = new DefaultSourceCompiler(false, ".js");

            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/emptyJS.js");

            var jsCompiler = Substitute.For<ICompiler>();
            jsCompiler.Compile(";", file).Returns<CompileResult>(info => { throw new CompileException("Error"); });

            var config = new CompilerConfiguration().CompilerFor(".js", jsCompiler).CompilerFor(".other", null) as CompilerConfiguration;

            var result = compiler.CompileFile(file, config).Result.Output;

            jsCompiler.Received().Compile(";", file);
            Assert.AreEqual("An error occurred during initial compilation: \r\nError", result);
        }

        [Test]
        public void CompileFile_ValidFile_MinificationPicksCorrectCompiler()
        {
            var compiler = new DefaultSourceCompiler(true, ".js");

            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/emptyJS.js");

            var minCompiler = Substitute.For<ICompiler>();
            minCompiler.Compile(";", null).Returns(Task.Factory.StartNew(() => new CompileResult { Output = ";" }));

            var config = new CompilerConfiguration().CompilerFor(".js", new PassthroughCompiler()).CompilerFor(".js.min", minCompiler) as CompilerConfiguration;

            var result = compiler.CompileFile(file, config).Result.Output;

            minCompiler.Received().Compile(";", null);
            Assert.AreEqual(";", result);
        }

        [Test]
        public void CompileFile_ValidFileWithDoesNotCompile_MinificationReturnsErrorInString()
        {
            var compiler = new DefaultSourceCompiler(true, ".js");

            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/emptyJS.js");

            var minCompiler = Substitute.For<ICompiler>();
            minCompiler.Compile(";", null).Returns<CompileResult>(info => { throw new CompileException("Error"); });

            var config = new CompilerConfiguration().CompilerFor(".js", new PassthroughCompiler()).CompilerFor(".js.min", minCompiler) as CompilerConfiguration;

            var result = compiler.CompileFile(file, config).Result.Output;

            minCompiler.Received().Compile(";", null);
            Assert.AreEqual("An error occurred during minification: \r\nError", result);
        }

        [Test]
        public void CompileFile_ValidMinJsFile_IgnoresMinificationStep()
        {
            var compiler = new DefaultSourceCompiler(true, ".js");

            var file = new FileInfo(TestContext.CurrentContext.TestDirectory + "/../../Data/emptyJS.min.js");

            var rightCompiler = Substitute.For<ICompiler>();
            rightCompiler.Compile(";", null).Returns(Task.Factory.StartNew(() => new CompileResult { Output = "Right" }));

            var wrongCompiler = Substitute.For<ICompiler>();
            wrongCompiler.Compile(";", null).Returns(Task.Factory.StartNew(() => new CompileResult { Output = "Wrong" }));

            var config = new CompilerConfiguration()
                .CompilerFor(".js", new PassthroughCompiler())
                .CompilerFor(".min.js.min", rightCompiler)
                .CompilerFor(".js.min", wrongCompiler) as CompilerConfiguration;

            var result = compiler.CompileFile(file, config).Result.Output;

            Assert.AreEqual("Right", result);
        }

        private CompilerConfiguration GetNodeDefConfig()
        {
            return new CompilerConfiguration().WithDefaultNodeConfiguration("Node") as CompilerConfiguration;
        }
    }
}
