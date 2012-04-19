﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using NodeAssets.Core.Commands;
using NodeAssets.Core.Compilers;
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
            _executor = new NodeExecutor("../../Node");
        }

        [Test]
        [ExpectedException(typeof(COMException))]
        public void Compile_InvalidStylusFile_Exception()
        {
            var styl = File.ReadAllText("../../Data/invalidStylus.styl");
            var compiler = new StylusCompiler(_executor, false);

            try
            {
                var output = compiler.Compile(styl).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        public void Compile_ValidStylusNoNib_Compiles()
        {
            var compiler = new StylusCompiler(_executor, false);
            var styl = File.ReadAllText("../../Data/exampleStylus.styl");

            var output = compiler.Compile(styl).Result;

            Assert.AreEqual(".base {\n  border-color: #ccc;\n}\n\n", output);
        }

        [Test]
        public void Compile_ValidStylusWithNib_Compiles()
        {
            var compiler = new StylusCompiler(_executor, true);
            var styl = File.ReadAllText("../../Data/exampleStylusWithNib.styl");

            var output = compiler.Compile(styl).Result;

            Assert.AreEqual(".base {\n  zoom: 1;\n}\n.base:before,\n.base:after {\n  content: \"\";\n  display: table;\n}\n.base:after {\n  clear: both;\n}\n\n", output);
        }
    }
}
