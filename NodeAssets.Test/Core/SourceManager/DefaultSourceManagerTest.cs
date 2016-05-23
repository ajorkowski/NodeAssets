﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NodeAssets.Core;
using NodeAssets.Core.SourceManager;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using NodeAssets.Compilers;

namespace NodeAssets.Test.Core.SourceManager
{
    [TestFixture]
    public class DefaultSourceManagerTest
    {
        private DefaultSourceManager _manager;
        private ISourceCompiler _sourceCompiler;

        [SetUp]
        public void Init()
        {
            _sourceCompiler = Substitute.For<ISourceCompiler>();
            _manager = new DefaultSourceManager(false, ".js", "testCompile", _sourceCompiler);
        }

        [TearDown]
        public void Done()
        {
            _manager.Dispose();
            Directory.Delete("testCompile", true);
        }

        [Test]
        public void CreateDefaultManager_Create_CreatesWorkingDirectory()
        {
            Assert.That(Directory.Exists("testCompile"));
        }

        [Test]
        public void SetPileAsSource_AddTwice_ExpectException()
        {
            var pile = Substitute.For<IPile>();
            pile.FindAllPiles().Returns(new List<string>());
            
            _manager.SetPileAsSource(pile, new CompilerConfiguration()).Wait();

            Assert.ThrowsAsync(typeof(InvalidOperationException), async () =>
            {
                await _manager.SetPileAsSource(pile, new CompilerConfiguration()).ConfigureAwait(false);
            });
        }

        [Test]
        public void SetPileAsSource_AddPile_CreatesAppropriateDirectories()
        {
            var pile = Substitute.For<IPile>();
            pile.FindAllPiles().Returns(new List<string> { "global", "other" });

            _manager.SetPileAsSource(pile, new CompilerConfiguration()).Wait();

            Assert.That(Directory.Exists("testCompile/global"));
            Assert.That(Directory.Exists("testCompile/other"));
        }

        [Test]
        public void SetPileAsSource_AddPile_DoesInitialCompile()
        {
            var pile = GetWorkingPile(false);
            var compiler = new CompilerConfiguration();
            _sourceCompiler.CompileFile(Arg.Any<FileInfo>(), compiler).Returns(Task.Factory.StartNew(() => new CompileResult { Output = string.Empty }));

            _manager.SetPileAsSource(pile, compiler).Wait();

            _sourceCompiler.Received(2).CompileFile(Arg.Any<FileInfo>(), compiler);
        }

        [Test]
        public void SetPileAsSource_AddPile_SavesResultOfCompile()
        {
            var pile = GetWorkingPile(false);
            var compiler = new CompilerConfiguration();
            _sourceCompiler.CompileFile(Arg.Any<FileInfo>(), compiler).Returns(Task.Factory.StartNew(() => new CompileResult { Output = "Saved" }));

            _manager.SetPileAsSource(pile, compiler).Wait();

            Assert.That(File.ReadAllText("testCompile/global/normalJavascript0.js"), Is.EqualTo("Saved"));
            Assert.That(File.ReadAllText("testCompile/coffee/exampleCoffee0.js"), Is.EqualTo("Saved"));
        }

        [Test]
        public void SetPileSource_AddPile_CominationWorks()
        {
            var manager = new DefaultSourceManager(true, ".js", "testCompile", _sourceCompiler);
            var pile = GetWorkingPile(true);
            var compiler = new CompilerConfiguration();
            _sourceCompiler.CompileFile(Arg.Any<FileInfo>(), compiler).Returns(Task.Factory.StartNew(() => new CompileResult { Output = "Saved" }));

            manager.SetPileAsSource(pile, compiler).Wait();

            Assert.That(File.ReadAllText("testCompile/global.js"), Is.EqualTo("SavedSaved"));
        }

        [Test]
        public void FindDestinationPile_WithoutAddingSource_ExpectException()
        {
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                _manager.FindDestinationPile();
            });
        }

        [Test]
        public void FindDestinationPile_WithRealPile_ReturnsCorrectSetOfFiles()
        {
            var pile = GetWorkingPile(true);
            var compiler = new CompilerConfiguration();
            _sourceCompiler.CompileFile(Arg.Any<FileInfo>(), compiler).Returns(Task.Factory.StartNew(() => new CompileResult { Output = "Saved" }));
            _manager.SetPileAsSource(pile, compiler).Wait();

            var dest = _manager.FindDestinationPile();

            Assert.That(dest.FindFiles("global").Count(), Is.EqualTo(2));
            Assert.That(dest.FindFiles("global").Any(f => f.Name == "exampleCoffee1.js"));
        }

        [Test]
        public void FindDestinationPile_WithRealPileCombined_ReturnsCorrectSetOfFiles()
        {
            var manager = new DefaultSourceManager(true, ".js", "testCompile", _sourceCompiler);
            var pile = GetWorkingPile(true);
            var compiler = new CompilerConfiguration();
            _sourceCompiler.CompileFile(Arg.Any<FileInfo>(), compiler).Returns(Task.Factory.StartNew(() => new CompileResult { Output = "Saved" }));
            manager.SetPileAsSource(pile, compiler).Wait();

            var dest = manager.FindDestinationPile();

            Assert.That(dest.FindFiles("global").Count(), Is.EqualTo(1));
            Assert.That(dest.FindFiles("global").Single().Name, Is.EqualTo("global.js"));
        }

        private IPile GetWorkingPile(bool bothGlobal)
        {
            var pile = new Pile(false).AddFile("../../Data/normalJavascript.js");

            if(bothGlobal)
            {
                pile.AddFile("../../Data/exampleCoffee.coffee");
            }
            else
            {
                pile.AddFile("coffee", "../../Data/exampleCoffee.coffee");
            }

            return pile;
        }
    }
}
