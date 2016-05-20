using System;
using System.IO;
using System.Runtime.InteropServices;
using NodeAssets.Compilers;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class CoffeeSharpCompilerTest
    {
        private CoffeeSharpCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new CoffeeSharpCompiler();
        }

        [Test]
        public void Compile_InvalidCoffeeFile_Exception()
        {
            Assert.ThrowsAsync(typeof(COMException), async () =>
            {
                var coffee = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/invalidCoffee.coffee");
                var output = await _compiler.Compile(coffee, null).ConfigureAwait(false);
            });
        }

        [Test]
        public void Compile_ValidCoffeeFile_Compiles()
        {
            var coffee = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleCoffee.coffee");

            var output = _compiler.Compile(coffee, null).Result;

            Assert.AreEqual("(function() {\n  var helloWorld;\n\n  helloWorld = function() {\n    return console.log('Hello World!');\n  };\n\n  helloWorld();\n\n}).call(this);\n", output);
        }
    }
}
