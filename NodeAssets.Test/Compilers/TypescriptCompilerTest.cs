using System.IO;
using NodeAssets.Compilers;
using NUnit.Framework;

namespace NodeAssets.Test.Core.Compilers
{
    [TestFixture]
    public class TypescriptCompilerTest
    {
        private TypescriptCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new TypescriptCompiler();
        }

        [Test]
        public void Compile_ValidTypescript_Compiles()
        {
            var file = new FileInfo("../../Data/examplets.ts");
            var ts = File.ReadAllText(file.FullName);

            var output = _compiler.Compile(ts, file).Result;

            Assert.AreEqual(@"var Shapes;
(function (Shapes) {
    var Point = (function () {
        function Point(x, y) {
            this.x = x;
            this.y = y;
        }
        Point.prototype.getDist = function () {
            return Math.sqrt(this.x * this.x + this.y * this.y);
        };
        Point.origin = new Point(0, 0);
        return Point;
    })();
    Shapes.Point = Point;    
})(Shapes || (Shapes = {}));
var p = new Shapes.Point(3, 4);
var dist = p.getDist();
", output);
        }
    }
}
