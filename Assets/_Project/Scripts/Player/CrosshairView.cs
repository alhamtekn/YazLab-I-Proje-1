using Lumen.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.Player
{
    /// <summary>
    /// Ekranın ortasındaki nişangah noktası (birinci şahısta nereye bakıldığı belli olsun).
    /// Yakında etkileşilebilir bir şey varsa büyür ve altın rengine döner; sohbet/menü açıkken gizlenir.
    /// </summary>
    public class CrosshairView : MonoBehaviour
    {
        [SerializeField] private Image dot;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private Color idle = new Color(1f, 1f, 1f, 0.55f);
        [SerializeField] private Color active = new Color(1f, 0.85f, 0.45f, 1f);

        public bool IsHighlighted { get; private set; }

        public void Configure(Image crosshair, PlayerInteractor playerInteractor)
        {
            dot = crosshair;
            interactor = playerInteractor;
        }

        private void Update()
        {
            dot.enabled = !InputLock.IsLocked;
            IsHighlighted = interactor.Current != null;

            float k = 1f - Mathf.Exp(-15f * Time.unscaledDeltaTime); // yumuşak geçiş
            dot.color = Color.Lerp(dot.color, IsHighlighted ? active : idle, k);
            dot.rectTransform.localScale = Vector3.Lerp(dot.rectTransform.localScale, Vector3.one * (IsHighlighted ? 1.8f : 1f), k);
        }
    }
}
