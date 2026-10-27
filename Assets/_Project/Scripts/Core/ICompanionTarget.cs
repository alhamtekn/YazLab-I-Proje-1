using UnityEngine;

namespace Lumen.Core
{
    /// <summary>
    /// Eşlikçinin (Lumi) "yardım et" emriyle kendi seçip gidebileceği hedefler.
    /// Hedefin nerede olduğu ve nasıl tetikleneceği hedefin kendisinde tanımlıdır;
    /// eşlikçi hangisine, hangi yoldan gideceğine NavMesh üzerinden kendisi karar verir.
    /// </summary>
    public interface ICompanionTarget
    {
        /// <summary>Eşlikçinin ulaşması gereken nokta.</summary>
        Vector3 ApproachPoint { get; }

        /// <summary>Hedef hâlâ yardım bekliyor mu (örn. şalter henüz indirilmedi mi).</summary>
        bool NeedsHelp { get; }

        /// <summary>Eşlikçi ApproachPoint'e ulaştığında çağrılır.</summary>
        void OnCompanionArrived(GameObject companion);
    }
}
