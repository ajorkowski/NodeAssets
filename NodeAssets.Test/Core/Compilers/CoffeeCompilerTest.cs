﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using NodeAssets.Core.Commands;
using NodeAssets.Core.Compilers;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class CoffeeCompilerTest
    {
        private CoffeeCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new CoffeeCompiler(new NodeExecutor("../../Node"));
        }

        [Test]
        [ExpectedException(typeof(COMException))]
        public void Compile_InvalidCoffeeFile_Exception()
        {
            try
            {
                var coffee = File.ReadAllText("../../Data/invalidCoffee.coffee");
                var output = _compiler.Compile(coffee, null).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        public void Compile_ValidCoffeeFile_Compiles()
        {
            var coffee = File.ReadAllText("../../Data/exampleCoffee.coffee");

            var output = _compiler.Compile(coffee, null).Result;

            Assert.AreEqual("// Generated by CoffeeScript 1.3.3\n(function() {\n  var helloWorld;\n\n  helloWorld = function() {\n    return console.log('Hello World!');\n  };\n\n  helloWorld();\n\n}).call(this);\n", output);
        }
    }
}
