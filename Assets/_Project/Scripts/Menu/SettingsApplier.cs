using UnityEngine;

namespace Lumen.Menu
{
    public interface ISettingsApplier
    {
        void Apply(GameSettings settings);
    }

    /// <summary>
    /// Ayarları motora uygular: ses = AudioListener.volume, ışık = sahnenin ortam ışığı çarpanı,
    /// tam ekran = Screen.fullScreen (editörde etkisi yoktur, yalnızca build'de görülür).
    /// </summary>
    public sealed class EngineSettingsApplier : ISettingsApplier
    {
        private readonly Color baseAmbient;

        /// <param name="sceneAmbient">Sahnenin tasarlanmış ortam ışığı; parlaklık bunun çarpanıdır.</param>
        public EngineSettingsApplier(Color sceneAmbient)
        {
            baseAmbient = sceneAmbient;
        }

        public void Apply(GameSettings settings)
        {
            AudioListener.volume = settings.volume;
            RenderSettings.ambientLight = baseAmbient * settings.brightness;

            if (Screen.fullScreen != settings.fullscreen)
            {
                Screen.fullScreen = settings.fullscreen;
            }
        }
    }
}
