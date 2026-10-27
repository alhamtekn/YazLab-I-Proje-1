using System;
using UnityEngine;

namespace Lumen.Menu
{
    /// <summary>
    /// Oyuncunun değiştirebildiği ayarlar. Yalnızca veri taşır; saklama ve uygulama ayrı sınıflardadır.
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public const float MinBrightness = 0.5f;
        public const float MaxBrightness = 4f;

        [Range(0f, 1f)] public float volume = 0.8f;
        [Range(MinBrightness, MaxBrightness)] public float brightness = 1f;
        public bool fullscreen = true;
    }

    public interface ISettingsStore
    {
        GameSettings Load();

        void Save(GameSettings settings);
    }

    /// <summary>Ayarları PlayerPrefs içinde saklar; oyun yeniden açıldığında korunur.</summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        private const string VolumeKey = "lumen.volume";
        private const string BrightnessKey = "lumen.brightness";
        private const string FullscreenKey = "lumen.fullscreen";

        public GameSettings Load()
        {
            var defaults = new GameSettings();
            return new GameSettings
            {
                volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, defaults.volume)),
                brightness = Mathf.Clamp(PlayerPrefs.GetFloat(BrightnessKey, defaults.brightness), GameSettings.MinBrightness, GameSettings.MaxBrightness),
                fullscreen = PlayerPrefs.GetInt(FullscreenKey, defaults.fullscreen ? 1 : 0) == 1
            };
        }

        public void Save(GameSettings settings)
        {
            PlayerPrefs.SetFloat(VolumeKey, settings.volume);
            PlayerPrefs.SetFloat(BrightnessKey, settings.brightness);
            PlayerPrefs.SetInt(FullscreenKey, settings.fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
