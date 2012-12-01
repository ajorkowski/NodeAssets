using NodeAssets.Compilers;
using NodeAssets.Core;
using NUnit.Framework;

namespace NodeAssets.Test.Core
{
    [TestFixture]
    public class CompilerConfigurationTest
    {
        private JsMinifyCompiler _comp1;
        private PassthroughCompiler _comp2;
        private CompilerConfiguration _config;

        [SetUp]
        public void Init()
        {
            _config = new CompilerConfiguration();
            _comp1 = new JsMinifyCompiler();
            _comp2 = new PassthroughCompiler();
        }

        [Test]
        public void CompilerFor_AddTwoMatches_UseFirstMatch()
        {
            _config.CompilerFor(".js", _comp2);
            _config.CompilerFor(".min.js.min", _comp2);
            _config.CompilerFor(".js.min", _comp1);

            var possible = _config.GetCompiler("blah.min.js.min");

            Assert.AreSame(_comp2, possible);
        }

        [Test]
        public void CompilerFor_AddExisting_AddsToSamePlace()
        {
            _config.CompilerFor(".js.min", new JsMinifyCompiler());
            _config.CompilerFor(".min.js.min", _comp2);
            _config.CompilerFor(".js.min", _comp1);

            var possible = _config.GetCompiler("blah.min.js.min");

            Assert.AreSame(_comp1, possible);
        }
    }
}
