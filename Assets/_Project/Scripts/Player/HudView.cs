using Lumen.Companion;
using Lumen.Level;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.Player
{
    /// <summary>
    /// Ekran bilgileri: kontroller, Lumi'nin anlık durumu, etkileşim ipucu ve kısa geri bildirimler.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] private Text companionStateText;
        [SerializeField] private Text promptText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private MonoBehaviour companion;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerCommandIssuer commandIssuer;
        [SerializeField] private CrystalCounter crystalCounter;
        [SerializeField] private float feedbackDuration = 2.5f;

        private ICompanionCommandReceiver receiver;
        private float feedbackTimer;

        public void Configure(Text state, Text prompt, Text feedback, MonoBehaviour companionReceiver, PlayerInteractor playerInteractor, PlayerCommandIssuer issuer)
        {
            companionStateText = state;
            promptText = prompt;
            feedbackText = feedback;
            companion = companionReceiver;
            interactor = playerInteractor;
            commandIssuer = issuer;
        }

        /// <summary>Kristal toplandıkça "Kristal 1/3" gibi kısa bir mesaj gösterilir.</summary>
        public void TrackCrystals(CrystalCounter counter)
        {
            crystalCounter = counter;
        }

        /// <summary>Diğer sistemlerin (örn. kristal toplama) kısa mesaj göstermesi için.</summary>
        public void ShowFeedback(string message)
        {
            feedbackText.text = message;
            feedbackTimer = feedbackDuration;
        }

        private void Awake()
        {
            receiver = companion as ICompanionCommandReceiver;
            feedbackText.text = string.Empty;
        }

        private void OnEnable()
        {
            if (commandIssuer != null)
            {
                commandIssuer.Feedback += ShowFeedback;
            }

            if (crystalCounter != null)
            {
                crystalCounter.CountChanged += ShowCrystalCount;
            }
        }

        private void OnDisable()
        {
            if (commandIssuer != null)
            {
                commandIssuer.Feedback -= ShowFeedback;
            }

            if (crystalCounter != null)
            {
                crystalCounter.CountChanged -= ShowCrystalCount;
            }
        }

        private void ShowCrystalCount(int collected, int required)
        {
            ShowFeedback(collected >= required
                ? $"Kristal {collected}/{required}: kapı açılıyor!"
                : $"Kristal {collected}/{required}");
        }

        private void Update()
        {
            if (receiver != null)
            {
                companionStateText.text = "Lumi: " + Describe(receiver.CurrentState);
            }

            promptText.text = interactor != null && interactor.Current != null ? interactor.Current.Prompt : string.Empty;

            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0f)
                {
                    feedbackText.text = string.Empty;
                }
            }
        }

        private static string Describe(CompanionStateId state) => state switch
        {
            CompanionStateId.Idle => "Bekliyor",
            CompanionStateId.Follow => "Seni takip ediyor",
            CompanionStateId.MoveToTarget => "Hedefe gidiyor",
            CompanionStateId.Avoid => "Tehlike algıladı, bekliyor",
            CompanionStateId.Interact => "Şalteri indiriyor",
            _ => state.ToString()
        };
    }
}
