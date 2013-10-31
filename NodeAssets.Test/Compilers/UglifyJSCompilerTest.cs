using System;
using System.IO;
using System.Runtime.InteropServices;
using NodeAssets.Compilers;
using NodeAssets.Core.Commands;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class UglifyJSCompilerTest
    {
        private UglifyJSCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new UglifyJSCompiler(new NodeExecutor("../../Node", "../../Node/node.exe"));
        }

        [Test]
        [ExpectedException(typeof(COMException))]
        public void Compile_InvalidUglifyJSFile_Exception()
        {
            try
            {
                var output = _compiler.Compile(File.ReadAllText("../../Data/invalidJS.js"), null).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        public void Compile_ValidUglifyJSFile_Compiles()
        {
            var js = File.ReadAllText("../../Data/normalJavascript.js");

            var output = _compiler.Compile(js, null).Result;

            Assert.AreEqual("var helloWorld=function(){console.log(\"Hello World\")};helloWorld();\n", output);
        }

        [Test]
        public void Compile_BigFile_Compiles()
        {
            var js = File.ReadAllText("../../Data/jquery-1.6.4.js");

            var output = _compiler.Compile(js, null).Result;

            Assert.AreNotEqual(string.Empty, output);
        }
    }
}
