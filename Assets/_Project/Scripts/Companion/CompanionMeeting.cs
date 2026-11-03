using System.Collections;
using Lumen.Core;
using Lumen.Player;
using UnityEngine;

namespace Lumen.Companion
{
    /// <summary>
    /// Giriş koridorunun sonunda Lumi ile tanışma. Lumi başta sönük ışıkla bekler; oyuncu yaklaşınca
    /// ışığı açılır, kendini tanıtır ve oyuncuyu takip etmeye başlar. Yalnızca bir kez olur.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CompanionMeeting : MonoBehaviour
    {
        [SerializeField] private CompanionController companion;
        [SerializeField] private HudView hud;
        [SerializeField] private Light companionLight;
        [SerializeField] private float dimIntensity = 0.35f;
        [SerializeField] private float wakeDuration = 1.2f;
        [SerializeField] private float messageSeconds = 8f;
        [SerializeField, TextArea] private string greeting =
            "Lumi: Merhaba yolcu! Ben Lumi, bu tapınağın son ışığıyım. Karanlıkta yolunu ben aydınlatırım.\n" +
            "Senin geçemediğin yerlere ben girerim: F ile yardım iste, G ile peşinden gelirim.";

        private float litIntensity;

        public bool HasMet { get; private set; }

        public void Configure(CompanionController companionController, HudView hudView, Light light)
        {
            companion = companionController;
            hud = hudView;
            companionLight = light;
        }

        private void Awake()
        {
            if (companionLight != null)
            {
                litIntensity = companionLight.intensity;
                companionLight.intensity = dimIntensity;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (HasMet || other.gameObject.layer != GameLayers.PlayerIndex)
            {
                return;
            }

            HasMet = true;
            hud.ShowFeedback(greeting, messageSeconds);
            companion.CommandFollow();
            StartCoroutine(Wake());
        }

        private IEnumerator Wake()
        {
            if (companionLight == null)
            {
                yield break;
            }

            for (float t = 0f; t < 1f; t += Time.deltaTime / wakeDuration)
            {
                companionLight.intensity = Mathf.Lerp(dimIntensity, litIntensity, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            companionLight.intensity = litIntensity;
        }
    }
}
