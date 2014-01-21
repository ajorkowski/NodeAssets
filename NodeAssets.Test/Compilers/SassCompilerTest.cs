﻿using NodeAssets.Compilers;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class SassCompilerTest
    {
        private SassCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new SassCompiler();
        }

        [Test]
        [ExpectedException(typeof(COMException))]
        public void Compile_InvalidSassFile_Exception()
        {
            var file = new FileInfo("../../Data/invalidSass.scss");
            var sass = File.ReadAllText(file.FullName);

            try
            {
                var output = _compiler.Compile(sass, file).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        public void Compile_ValidSass_Compiles()
        {
            var file = new FileInfo("../../Data/exampleSass.scss");
            var sass = File.ReadAllText(file.FullName);

            var output = _compiler.Compile(sass, file).Result;

            Assert.AreEqual("/* line 1, source string */\n.base {\n  border-color: #cccccc; }\n", output);
        }

        [Test]
        public void Compile_ValidSassWithImport_Compiles()
        {
            var file = new FileInfo("../../Data/exampleSassWithImport.scss");
            var styl = File.ReadAllText(file.FullName);

            var output = _compiler.Compile(styl, file).Result;

            Assert.AreEqual("/* line 5, source string */\n.base {\n  border-color: #cccccc;\n  width: auto; }\n", output);
        }
    }
}