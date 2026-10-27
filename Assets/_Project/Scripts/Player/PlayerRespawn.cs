using Lumen.Core;
using UnityEngine;

namespace Lumen.Player
{
    /// <summary>
    /// Oyuncuyu son kontrol noktasına geri getirir.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerRespawn : MonoBehaviour, IRespawnable
    {
        private PlayerController controller;
        private Vector3 checkpoint;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            checkpoint = transform.position;
        }

        public void SetCheckpoint(Vector3 position)
        {
            checkpoint = position;
        }

        public void Respawn()
        {
            controller.Teleport(checkpoint);
        }
    }
}
