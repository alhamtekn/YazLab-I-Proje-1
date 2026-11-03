using UnityEngine;

namespace Lumen.Menu
{
    /// <summary>
    /// Ana menüdeki ışık küresi: çekirdek ve loş hale nefes alır gibi parlayıp söner;
    /// dışarıda soluk bir ışık halkası yavaşça genişleyip kaybolur, sonra baştan başlar.
    /// </summary>
    public class LightOrbView : MonoBehaviour
    {
        [SerializeField] private RectTransform core;
        [SerializeField] private CanvasGroup halo;
        [SerializeField] private RectTransform ring;
        [SerializeField] private CanvasGroup ringGroup;
        [SerializeField] private float pulseSpeed = 1.2f;
        [SerializeField] private float ringSeconds = 4f;  // halkanın bir kez genişleyip kaybolma süresi

        public void Configure(RectTransform coreRect, CanvasGroup haloGroup, RectTransform ringRect, CanvasGroup ringCanvasGroup)
        {
            core = coreRect;
            halo = haloGroup;
            ring = ringRect;
            ringGroup = ringCanvasGroup;
        }

        private void Update()
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
            core.localScale = Vector3.one * (0.95f + 0.1f * pulse);
            halo.alpha = 0.35f + 0.25f * pulse;

            // t: 0 → 1 arası döner; halka büyüdükçe söner
            float t = Mathf.Repeat(Time.unscaledTime / ringSeconds, 1f);
            ring.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.6f, t);
            ringGroup.alpha = 0.35f * Mathf.Sin(t * Mathf.PI); // yavaşça belirir, yavaşça kaybolur
        }
    }
}
