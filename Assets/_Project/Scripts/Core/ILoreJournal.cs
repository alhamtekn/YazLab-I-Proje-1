using System;
using System.Collections.Generic;

namespace Lumen.Core
{
    /// <summary>
    /// Oyuncunun ara bölgede (kristal galerisi) keşfettiği yazıtların kaydı.
    /// Seviye tarafı yazar, NPC tarafı diyalog bağlamına eklemek için yalnızca okur.
    /// </summary>
    public interface ILoreJournal
    {
        IReadOnlyList<string> Entries { get; }

        event Action<string> EntryAdded;
    }
}
