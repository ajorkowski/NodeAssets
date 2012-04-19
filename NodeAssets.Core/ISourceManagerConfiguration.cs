using NodeAssets.Core.SourceManager;

namespace NodeAssets.Core
{
    public interface ISourceManagerConfiguration
    {
        ISourceManagerConfiguration UseDefaultConfiguration(string outputDir, bool isProduction);
        ISourceManagerConfiguration Minimise(bool minimise);
        ISourceManagerConfiguration Combine(bool combine);
        ISourceManagerConfiguration OutputTo(string outputDir);

        ISourceManager GetSourceManager(string extension);
    }
}
