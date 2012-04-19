namespace NodeAssets.Core.SourceManager
{
    public interface ISourceManager
    {
        void SetPileAsSource(IPile pile, ICompilerConfiguration compilerManager);
        IPile FindDestinationPile();
    }
}
