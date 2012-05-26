using System;
using NodeAssets.Core.SourceManager;

namespace NodeAssets.Core
{
    public class SourceManagerConfiguration : ISourceManagerConfiguration
    {
        private string _outputDir;
        private bool _minimise;
        private bool _combine;

        public ISourceManagerConfiguration UseDefaultConfiguration(string outputDir, bool isProduction)
        {
            _outputDir = outputDir;
            _minimise = isProduction;
            _combine = isProduction;
            return this;
        }

        public ISourceManagerConfiguration Minimise(bool minimise)
        {
            _minimise = minimise;
            return this;
        }

        public ISourceManagerConfiguration Combine(bool combine)
        {
            _combine = combine;
            return this;
        }

        public ISourceManagerConfiguration OutputTo(string outputDir)
        {
            _outputDir = outputDir;
            return this;
        }

        public ISourceManager GetSourceManager(string extension)
        {
            return new DefaultSourceManager(_combine, extension, _outputDir, new DefaultSourceCompiler(_minimise, extension));
        }
    }
}
