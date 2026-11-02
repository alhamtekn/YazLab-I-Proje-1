using Lumen.Core;
using UnityEngine;

namespace Lumen.NPC
{
    // Oyun bitince ekranı kaplayan bitiş paneli. Oyun durumunu yalnızca dinler.
    public sealed class EndScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private GameStateManager gameState;

        public void Configure(GameObject endPanel, GameStateManager state)
        {
            panel = endPanel;
            gameState = state;
        }

        private void Awake() => panel.SetActive(false);

        private void OnEnable() => gameState.GameEnded += Show;

        private void OnDisable() => gameState.GameEnded -= Show;

        private void OnDestroy() => InputLock.Release(this);

        private void Show()
        {
            panel.SetActive(true);
            InputLock.Acquire(this); // oyun bitti: karakter durur, imleç serbest (Esc menüsü çalışmaya devam eder)
        }
    }
}
