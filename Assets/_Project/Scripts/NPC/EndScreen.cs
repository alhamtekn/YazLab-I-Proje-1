using Lumen.Core;
using Lumen.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.NPC
{
    // Oyun bitince ekranı kaplayan bitiş paneli: oyun süresi, Arven'in son güveni ve ana menüye dönüş. Oyun durumunu yalnızca dinler.
    public sealed class EndScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private GameStateManager gameState;
        [SerializeField] private Text statsText;
        [SerializeField] private Button mainMenuButton;

        public void Configure(GameObject endPanel, GameStateManager state, Text stats, Button mainMenu)
        {
            panel = endPanel;
            gameState = state;
            statsText = stats;
            mainMenuButton = mainMenu;
        }

        /// <summary>Süreyi "dakika:saniye" olarak yazar (test için ayrı).</summary>
        public static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }

        private void Awake()
        {
            panel.SetActive(false);
            mainMenuButton.onClick.AddListener(() => new BuildSettingsSceneNavigator().TryLoadMainMenu());
        }

        private void OnEnable() => gameState.GameEnded += Show;

        private void OnDisable() => gameState.GameEnded -= Show;

        private void OnDestroy() => InputLock.Release(this);

        private void Show()
        {
            // Duraklatma menüsünde geçen süre sayılmaz (timeSinceLevelLoad ölçekli zamandır)
            statsText.text = $"Süre: {FormatTime(Time.timeSinceLevelLoad)}    Arven'in güveni: {gameState.Trust}";
            panel.SetActive(true);
            InputLock.Acquire(this); // oyun bitti: karakter durur, imleç serbest
        }
    }
}
