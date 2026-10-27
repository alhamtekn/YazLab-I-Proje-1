using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>İçinden geçen oyuncunun dönüş noktasını günceller.</summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            other.GetComponentInParent<IRespawnable>()?.SetCheckpoint(transform.position);
        }
    }
}
