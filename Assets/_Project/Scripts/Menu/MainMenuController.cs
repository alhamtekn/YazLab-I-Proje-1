using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lumen.Menu
{
    /// <summary>
    /// Ana menü: Oyna oyunu başlatır, Ayarlar ses/ışık/tam ekran panelini açar, Çıkış oyunu kapatır.
    /// Menü siyahtan yavaşça belirir; Oyna'ya basınca kararıp oyun sahnesine geçer.
    /// Ayarlar duraklatma menüsüyle aynı yerde (PlayerPrefs) saklanır; oyunda da geçerlidir.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        public const string GameScene = "Lumen";

        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button backButton;
        [SerializeField] private CanvasGroup fader;      // ekranı kaplayan siyah perde
        [SerializeField] private float fadeSeconds = 1.2f;

        private ISettingsStore store;
        private ISettingsApplier applier;
        private GameSettings settings;
        private bool starting;

        public void Configure(Button play, Button settingsOpen, Button quit, GameObject panel, Slider volume, Slider brightness, Toggle fullscreen, Button back, CanvasGroup fade)
        {
            playButton = play;
            settingsButton = settingsOpen;
            quitButton = quit;
            settingsPanel = panel;
            volumeSlider = volume;
            brightnessSlider = brightness;
            fullscreenToggle = fullscreen;
            backButton = back;
            fader = fade;
        }

        private void Awake()
        {
            // Oyundan dönülmüş olabilir: zaman akmalı, imleç görünmeli
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            store = new PlayerPrefsSettingsStore();
            applier = new EngineSettingsApplier(RenderSettings.ambientLight);
            settings = store.Load();
            applier.Apply(settings);

            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            brightnessSlider.minValue = GameSettings.MinBrightness;
            brightnessSlider.maxValue = GameSettings.MaxBrightness;
            volumeSlider.SetValueWithoutNotify(settings.volume);
            brightnessSlider.SetValueWithoutNotify(settings.brightness);
            fullscreenToggle.SetIsOnWithoutNotify(settings.fullscreen);

            playButton.onClick.AddListener(Play);
            settingsButton.onClick.AddListener(() => settingsPanel.SetActive(true));
            quitButton.onClick.AddListener(Application.Quit);
            backButton.onClick.AddListener(CloseSettings);
            volumeSlider.onValueChanged.AddListener(v => { settings.volume = v; applier.Apply(settings); });
            brightnessSlider.onValueChanged.AddListener(v => { settings.brightness = v; applier.Apply(settings); });
            fullscreenToggle.onValueChanged.AddListener(v => { settings.fullscreen = v; applier.Apply(settings); });

            settingsPanel.SetActive(false);
            StartCoroutine(Fade(1f, 0f)); // siyahtan aç
        }

        private void Update()
        {
            if (settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseSettings();
            }
        }

        private void Play()
        {
            if (starting)
            {
                return; // iki kez basılırsa sahne iki kez yüklenmesin
            }

            starting = true;
            StartCoroutine(FadeThenLoad());
        }

        private IEnumerator FadeThenLoad()
        {
            yield return Fade(0f, 1f);
            SceneManager.LoadScene(GameScene);
        }

        private IEnumerator Fade(float from, float to)
        {
            fader.blocksRaycasts = true; // geçiş sırasında tıklama alınmasın
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / fadeSeconds)
            {
                fader.alpha = Mathf.Lerp(from, to, t);
                // müzik de perdeyle birlikte kısılır/açılır
                AudioListener.volume = settings.volume * (1f - fader.alpha);
                yield return null;
            }

            fader.alpha = to;
            AudioListener.volume = settings.volume * (1f - to);
            fader.blocksRaycasts = to > 0f;
        }

        private void CloseSettings()
        {
            settingsPanel.SetActive(false);
            store.Save(settings);
        }
    }
}
