using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Düşme/yasak bölge tetiği. Kaybetme sebebi verilmişse ve oyun durumu bağlıysa içine giren oyuncu kaybeder
    /// (lav, Lumi'nin parkuru); yoksa eskisi gibi son kontrol noktasına döner. Lumi ve diğer nesneler etkilenmez.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        [SerializeField] private string loseReason;
        [SerializeField] private MonoBehaviour gameOverSource; // IGameOver

        private IGameOver gameOver;

        public string LoseReason => loseReason;

        public KillZone SetLoseReason(string reason)
        {
            loseReason = reason;
            return this;
        }

        public void SetGameOver(MonoBehaviour source) => gameOverSource = source;

        private void Awake() => gameOver = gameOverSource as IGameOver;

        private void OnTriggerEnter(Collider other)
        {
            if (gameOver != null && !string.IsNullOrEmpty(loseReason))
            {
                if (other.gameObject.layer == GameLayers.PlayerIndex)
                {
                    gameOver.Lose(loseReason);
                }

                return;
            }

            other.GetComponentInParent<IRespawnable>()?.Respawn();
        }
    }
}
