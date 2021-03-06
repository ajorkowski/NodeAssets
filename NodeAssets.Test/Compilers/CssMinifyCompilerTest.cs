﻿using NodeAssets.Compilers;
using NUnit.Framework;
using System.IO;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class CssMinifyCompilerTest
    {
        private CssMinifyCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new CssMinifyCompiler();
        }

        [Test]
        public void Compile_ValidCSSFile_Compiles()
        {
            var css = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "/../../Data/exampleCss.css");

            var result = _compiler.Compile(css, null).Result.Output;

            Assert.AreEqual(".test,.test{color:aqua;color:#fff}", result);
        }
    }
}
