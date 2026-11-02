using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lumen.Menu
{
    /// <summary>
    /// Ana menü: Oyna oyunu başlatır, Ayarlar ses/ışık/tam ekran panelini açar.
    /// Ayarlar duraklatma menüsüyle aynı yerde (PlayerPrefs) saklanır; oyunda da geçerlidir.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        public const string GameScene = "Lumen";

        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button backButton;

        private ISettingsStore store;
        private ISettingsApplier applier;
        private GameSettings settings;

        public void Configure(Button play, Button settingsOpen, GameObject panel, Slider volume, Slider brightness, Toggle fullscreen, Button back)
        {
            playButton = play;
            settingsButton = settingsOpen;
            settingsPanel = panel;
            volumeSlider = volume;
            brightnessSlider = brightness;
            fullscreenToggle = fullscreen;
            backButton = back;
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

            playButton.onClick.AddListener(() => SceneManager.LoadScene(GameScene));
            settingsButton.onClick.AddListener(() => settingsPanel.SetActive(true));
            backButton.onClick.AddListener(CloseSettings);
            volumeSlider.onValueChanged.AddListener(v => { settings.volume = v; applier.Apply(settings); });
            brightnessSlider.onValueChanged.AddListener(v => { settings.brightness = v; applier.Apply(settings); });
            fullscreenToggle.onValueChanged.AddListener(v => { settings.fullscreen = v; applier.Apply(settings); });

            settingsPanel.SetActive(false);
        }

        private void Update()
        {
            if (settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseSettings();
            }
        }

        private void CloseSettings()
        {
            settingsPanel.SetActive(false);
            store.Save(settings);
        }
    }
}
