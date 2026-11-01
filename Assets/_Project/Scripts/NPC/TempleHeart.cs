using Lumen.Core;
using UnityEngine;

namespace Lumen.NPC
{
    // Arven'in kapısının ardındaki tapınağın kalbi. Oyuncu içine girince oyun biter.
    [RequireComponent(typeof(Collider))]
    public sealed class TempleHeart : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameState;

        public void Configure(GameStateManager state) => gameState = state;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == GameLayers.PlayerIndex)
            {
                gameState.EndGame();
            }
        }
    }
}
