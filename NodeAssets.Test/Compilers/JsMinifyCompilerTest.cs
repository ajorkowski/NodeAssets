using System;
using System.IO;
using System.Runtime.InteropServices;
using NodeAssets.Compilers;
using NUnit.Framework;

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
        [ExpectedException(typeof(COMException))]
        public void Compile_InvalidJsMinifyFile_Exception()
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
        public void Compile_ValidJsMinifyFile_Compiles()
        {
            var js = File.ReadAllText("../../Data/normalJavascript.js");

            var output = _compiler.Compile(js, null).Result;

            Assert.AreEqual("var helloWorld=function(){console.log(\"Hello World\")};helloWorld();", output);
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
