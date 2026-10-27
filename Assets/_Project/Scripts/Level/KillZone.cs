using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>Uçurum dibindeki tetik: içine düşeni son kontrol noktasına döndürür.</summary>
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            other.GetComponentInParent<IRespawnable>()?.Respawn();
        }
    }
}
