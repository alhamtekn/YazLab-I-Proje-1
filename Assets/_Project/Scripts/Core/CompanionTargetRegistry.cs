using System.Collections.Generic;
using UnityEngine;

namespace Lumen.Core
{
    /// <summary>
    /// Sahnedeki eşlikçi hedeflerinin kaydı. Hedefler kendilerini OnEnable/OnDisable içinde
    /// kaydeder; eşlikçi hangi hedefin seçileceğine bu liste üzerinden karar verir.
    /// </summary>
    public static class CompanionTargetRegistry
    {
        private static readonly List<ICompanionTarget> Targets = new List<ICompanionTarget>();

        public static IReadOnlyList<ICompanionTarget> All => Targets;

        public static void Register(ICompanionTarget target)
        {
            if (!Targets.Contains(target))
            {
                Targets.Add(target);
            }
        }

        public static void Unregister(ICompanionTarget target)
        {
            Targets.Remove(target);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Targets.Clear();
        }
    }
}
