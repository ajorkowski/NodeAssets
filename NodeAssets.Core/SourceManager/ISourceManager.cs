﻿using System.Threading.Tasks;

namespace NodeAssets.Core.SourceManager
{
    public interface ISourceManager
    {
        Task SetPileAsSource(IPile pile, ICompilerConfiguration compilerManager);
        IPile FindDestinationPile();
    }
}
