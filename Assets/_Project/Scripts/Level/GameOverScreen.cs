using System.Collections;
using Lumen.Core;
using Lumen.Menu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lumen.Level
{
    /// <summary>
    /// "Şansını kaybettin" ekranı: kaybedilince siyahtan açılır, sebebi yazar. "Yeniden Oyna" sahneyi baştan yükler,
    /// "Ana Menü" menüye döner. Oyun durumunu IGameOver üzerinden dinler, NPC koduna bağlı değildir.
    /// </summary>
    public sealed class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour gameOverSource; // IGameOver
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text reasonText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private float fadeSeconds = 0.8f;

        private IGameOver gameOver;

        public bool IsOpen => group.gameObject.activeSelf;
        public Button RetryButton => retryButton;
        public string Reason => reasonText.text;

        public void Configure(MonoBehaviour source, CanvasGroup panelGroup, Text reason, Button retry, Button mainMenu)
        {
            gameOverSource = source;
            group = panelGroup;
            reasonText = reason;
            retryButton = retry;
            mainMenuButton = mainMenu;
        }

        private void Awake()
        {
            gameOver = gameOverSource as IGameOver;
            group.gameObject.SetActive(false);
            retryButton.onClick.AddListener(Retry);
            mainMenuButton.onClick.AddListener(MainMenu);
        }

        private void OnEnable()
        {
            if (gameOver != null)
            {
                gameOver.GameLost += Show;
            }
        }

        private void OnDisable()
        {
            if (gameOver != null)
            {
                gameOver.GameLost -= Show;
            }
        }

        private void OnDestroy() => InputLock.Release(this);

        private void Show(string reason)
        {
            reasonText.text = reason;
            group.alpha = 0f;
            group.gameObject.SetActive(true);
            InputLock.Acquire(this); // karakter durur, imleç serbest
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                group.alpha = t / fadeSeconds;
                yield return null;
            }

            group.alpha = 1f;
        }

        private void Retry()
        {
            Time.timeScale = 1f;
            InputLock.Release(this);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void MainMenu()
        {
            Time.timeScale = 1f;
            InputLock.Release(this);
            new BuildSettingsSceneNavigator().TryLoadMainMenu();
        }
    }
}
