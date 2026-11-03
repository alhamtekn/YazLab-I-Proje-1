using Lumen.Core;
using Lumen.Player;
using UnityEngine;

namespace Lumen.Sanctum
{
    /// <summary>Oyuncu bölgeye ilk girdiğinde HUD'da kısa bir ipucu gösterir (bir kez).</summary>
    [RequireComponent(typeof(Collider))]
    public class ZoneHint : MonoBehaviour
    {
        [SerializeField] private HudView hud;
        [SerializeField, TextArea] private string message;

        private bool shown;

        public void Configure(HudView hudView, string text)
        {
            hud = hudView;
            message = text;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (shown || other.gameObject.layer != GameLayers.PlayerIndex)
            {
                return;
            }

            shown = true;
            hud.ShowFeedback(message, 7f);
        }
    }
}
