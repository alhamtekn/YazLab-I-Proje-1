using UnityEngine;

namespace Lumen.Menu
{
    /// <summary>
    /// Ana menüdeki ışık küresi: ışınlar yavaşça döner, çekirdek ve loş hale nefes alır gibi parlayıp söner.
    /// </summary>
    public class LightOrbView : MonoBehaviour
    {
        [SerializeField] private RectTransform rays;
        [SerializeField] private RectTransform core;
        [SerializeField] private CanvasGroup halo;
        [SerializeField] private float raySpeed = 6f;   // derece/sn
        [SerializeField] private float pulseSpeed = 1.2f;

        public void Configure(RectTransform rayRoot, RectTransform coreRect, CanvasGroup haloGroup)
        {
            rays = rayRoot;
            core = coreRect;
            halo = haloGroup;
        }

        private void Update()
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
            rays.Rotate(0f, 0f, -raySpeed * Time.unscaledDeltaTime);
            core.localScale = Vector3.one * (0.95f + 0.1f * pulse);
            halo.alpha = 0.35f + 0.25f * pulse;
        }
    }
}
