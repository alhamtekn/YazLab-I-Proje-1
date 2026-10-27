using UnityEngine;

namespace Lumen.Core
{
    /// <summary>
    /// Düşme bölgesine giren ve son kontrol noktasına geri dönebilen varlıklar.
    /// </summary>
    public interface IRespawnable
    {
        void Respawn();

        void SetCheckpoint(Vector3 position);
    }
}
