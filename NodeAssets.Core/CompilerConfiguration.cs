﻿using System;
using System.Collections.Generic;
using System.Linq;
using NodeAssets.Compilers;

namespace NodeAssets.Core
{
    public sealed class CompilerConfiguration : ICompilerConfiguration
    {
        private readonly List<KeyValuePair<string, ICompiler>> _compilers;
        private Action<Exception> _onCompilerError;
        private Action<CompileMeasurement> _onCompileMeasure;

        public CompilerConfiguration()
        {
            _compilers = new List<KeyValuePair<string, ICompiler>>();
            _onCompilerError = null;
            _onCompileMeasure = null;
        }

        public ICompilerConfiguration CompilerFor(string extension, ICompiler compiler)
        {
            var possible = _compilers.SingleOrDefault(c => string.Compare(c.Key, extension, StringComparison.OrdinalIgnoreCase) == 0);

            var newComp = new KeyValuePair<string, ICompiler>(extension, compiler);
            if (possible.Key != null)
            {
                var index = _compilers.IndexOf(possible);
                _compilers.Remove(possible);
                _compilers.Insert(index, newComp);
            }
            else
            {
                _compilers.Add(newComp);
            }
            
            return this;
        }

        public ICompilerConfiguration OnCompilerError(Action<Exception> onErrorFunc)
        {
            _onCompilerError = onErrorFunc;
            return this;
        }

        public ICompilerConfiguration OnCompileMeasurement(Action<CompileMeasurement> onMeasure)
        {
            _onCompileMeasure = onMeasure;
            return this;
        }

        public ICompiler GetCompiler(string extension)
        {
            ICompiler result = null;

            foreach (var compiler in _compilers)
            {
                if (extension.EndsWith(compiler.Key, StringComparison.OrdinalIgnoreCase))
                {
                    result = compiler.Value;
                    break;
                }
            }

            return result;
        }

        public void HasException(Exception e)
        {
            if (_onCompilerError != null)
            {
                _onCompilerError(e);
            }
        }

        public void HasMeasurement(CompileMeasurement c)
        {
            if (_onCompileMeasure != null)
            {
                _onCompileMeasure(c);
            }
        }
    }
}
