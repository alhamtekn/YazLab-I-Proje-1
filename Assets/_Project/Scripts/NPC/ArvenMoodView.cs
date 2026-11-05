using UnityEngine;

namespace Lumen.NPC
{
    /// <summary>
    /// Arven'in gözleri modelin döndürdüğü ruh haline (mood) göre renk değiştirir:
    /// sakin mavi, dostça yeşil, şüpheli sarı, öfkeli kırmızı. Renk yarım saniyede yumuşakça geçer.
    /// Modelin yapılandırılmış çıktısındaki bir alanın oyunda görünür bir sonucu olur.
    /// </summary>
    public sealed class ArvenMoodView : MonoBehaviour
    {
        [SerializeField] private ArvenDialogue dialogue;
        [SerializeField] private Renderer[] eyes;
        [SerializeField] private Light eyeLight;
        [SerializeField] private float blendSeconds = 0.5f;

        private Color current;
        private Color target;
        private MaterialPropertyBlock block;

        public string CurrentMood { get; private set; } = "neutral";

        public void Configure(ArvenDialogue arvenDialogue, Renderer[] eyeRenderers, Light glow)
        {
            dialogue = arvenDialogue;
            eyes = eyeRenderers;
            eyeLight = glow;
        }

        /// <summary>Ruh halinin rengi; listede olmayan değer sakin (mavi) sayılır.</summary>
        public static Color ColorOf(string mood) => mood switch
        {
            "friendly" => new Color(0.4f, 1f, 0.5f),
            "suspicious" => new Color(1f, 0.8f, 0.25f),
            "angry" => new Color(1f, 0.2f, 0.15f),
            _ => new Color(0.5f, 0.8f, 1f)
        };

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            current = target = ColorOf(CurrentMood);
            Apply();
        }

        private void OnEnable() => dialogue.Responded += OnResponded;

        private void OnDisable() => dialogue.Responded -= OnResponded;

        private void OnResponded(NPCResponse response)
        {
            CurrentMood = response.mood;
            target = ColorOf(response.mood);
        }

        private void Update()
        {
            if (current == target)
            {
                return;
            }

            current = Color.Lerp(current, target, Mathf.Clamp01(Time.deltaTime / blendSeconds * 4f));
            if (Vector4.Distance(current, target) < 0.01f)
            {
                current = target;
            }

            Apply();
        }

        private void Apply()
        {
            block.SetColor("_Color", current);
            block.SetColor("_EmissionColor", current * 2.4f); // gözler parlasın
            foreach (Renderer eye in eyes)
            {
                eye.SetPropertyBlock(block);
            }

            eyeLight.color = current;
        }
    }
}
