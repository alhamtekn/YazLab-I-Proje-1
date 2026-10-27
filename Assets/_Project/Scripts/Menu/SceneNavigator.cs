using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lumen.Menu
{
    public interface ISceneNavigator
    {
        /// <summary>Ana menü sahnesine geçer. Sahne henüz yoksa false döner.</summary>
        bool TryLoadMainMenu();
    }

    /// <summary>
    /// Build ayarlarındaki "MainMenu" sahnesini yükler. Ana menü tasarlanıp build listesine eklenene kadar
    /// false döner ve oyun olduğu yerde kalır.
    /// </summary>
    public sealed class BuildSettingsSceneNavigator : ISceneNavigator
    {
        public const string MainMenuScene = "MainMenu";

        public bool TryLoadMainMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(MainMenuScene))
            {
                return false;
            }

            SceneManager.LoadScene(MainMenuScene);
            return true;
        }
    }
}
