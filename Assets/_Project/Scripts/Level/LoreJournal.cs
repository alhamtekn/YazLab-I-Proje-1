using System;
using System.Collections.Generic;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Oyuncunun okuduğu yazıtların kaydı. Aynı metin iki kez eklenmez;
    /// her yeni kayıtta EntryAdded tetiklenir. NPC tarafı ILoreJournal üzerinden okur.
    /// </summary>
    public class LoreJournal : MonoBehaviour, ILoreJournal
    {
        private readonly List<string> entries = new List<string>();

        public event Action<string> EntryAdded;

        public IReadOnlyList<string> Entries => entries;

        /// <summary>Metni günlüğe ekler; boşsa ya da zaten varsa false döner.</summary>
        public bool Add(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry) || entries.Contains(entry))
            {
                return false;
            }

            entries.Add(entry);
            EntryAdded?.Invoke(entry);
            return true;
        }
    }
}
