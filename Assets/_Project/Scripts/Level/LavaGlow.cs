using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Lav yüzeyinin yavaş nabzı: ışıma rengi ve alttan duvarları aydınlatan ışıklar birlikte, hafif kayık
    /// iki dalgayla parlayıp söner. Paylaşılan "Lava" malzemesine dokunmamak için MaterialPropertyBlock kullanır.
    /// </summary>
    public class LavaGlow : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Renderer surface;
        [SerializeField] private Light[] lights = System.Array.Empty<Light>();
        [SerializeField] private Color emission = new Color(3f, 0.9f, 0.15f);
        [SerializeField] private float pulseSpeed = 0.6f;
        [SerializeField] private float pulseAmount = 0.25f;

        private MaterialPropertyBlock block;
        private float[] baseIntensities;

        public void Configure(Renderer lavaSurface, Light[] lavaLights, Color lavaEmission)
        {
            surface = lavaSurface;
            lights = lavaLights;
            emission = lavaEmission;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            baseIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                baseIntensities[i] = lights[i].intensity;
            }
        }

        private void Update()
        {
            float t = Time.time * pulseSpeed;
            float pulse = 1f + pulseAmount * (0.6f * Mathf.Sin(t) + 0.4f * Mathf.Sin(t * 2.3f + 1.1f));

            if (surface != null)
            {
                surface.GetPropertyBlock(block);
                block.SetColor(EmissionColorId, emission * pulse);
                surface.SetPropertyBlock(block);
            }

            for (int i = 0; i < lights.Length; i++)
            {
                // Her ışık biraz farklı fazda titrer; yüzey canlı görünür.
                lights[i].intensity = baseIntensities[i] * (1f + pulseAmount * Mathf.Sin(t * 1.7f + i * 2f));
            }
        }
    }
}
