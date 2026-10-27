using Lumen.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.Menu
{
    /// <summary>
    /// Esc ile açılan duraklatma menüsü: Devam Et, Ses, Işık, Tam Ekran, Ana Menü.
    /// Açıkken oyun zamanı durur ve oyuncu girdisi kilitlenir (imleç serbest kalır).
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Text statusText;
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        private ISettingsStore store;
        private ISettingsApplier applier;
        private ISceneNavigator navigator;
        private GameSettings settings;
        private float timeScaleBeforePause = 1f;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Configure(GameObject menuPanel, Button resume, Slider volume, Slider brightness, Toggle fullscreen, Button mainMenu, Text status)
        {
            panel = menuPanel;
            resumeButton = resume;
            volumeSlider = volume;
            brightnessSlider = brightness;
            fullscreenToggle = fullscreen;
            mainMenuButton = mainMenu;
            statusText = status;
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            panel.SetActive(true);
            statusText.text = string.Empty;
            InputLock.Acquire(this);
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            panel.SetActive(false);
            Time.timeScale = timeScaleBeforePause;
            InputLock.Release(this);
            store.Save(settings);
        }

        private void Awake()
        {
            store = new PlayerPrefsSettingsStore();
            applier = new EngineSettingsApplier(RenderSettings.ambientLight);
            navigator = new BuildSettingsSceneNavigator();
            settings = store.Load();
            applier.Apply(settings);

            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            brightnessSlider.minValue = GameSettings.MinBrightness;
            brightnessSlider.maxValue = GameSettings.MaxBrightness;
            volumeSlider.SetValueWithoutNotify(settings.volume);
            brightnessSlider.SetValueWithoutNotify(settings.brightness);
            fullscreenToggle.SetIsOnWithoutNotify(settings.fullscreen);

            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            resumeButton.onClick.AddListener(Close);
            mainMenuButton.onClick.AddListener(OnMainMenu);

            panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (IsOpen)
            {
                Time.timeScale = timeScaleBeforePause;
                InputLock.Release(this);
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(toggleKey))
            {
                return;
            }

            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void OnVolumeChanged(float value)
        {
            settings.volume = value;
            applier.Apply(settings);
        }

        private void OnBrightnessChanged(float value)
        {
            settings.brightness = value;
            applier.Apply(settings);
        }

        private void OnFullscreenChanged(bool value)
        {
            settings.fullscreen = value;
            applier.Apply(settings);
        }

        private void OnMainMenu()
        {
            store.Save(settings);
            Time.timeScale = 1f;
            InputLock.Release(this);

            if (!navigator.TryLoadMainMenu())
            {
                Time.timeScale = 0f;
                InputLock.Acquire(this);
                statusText.text = "Ana menü henüz eklenmedi.";
            }
        }
    }
}
