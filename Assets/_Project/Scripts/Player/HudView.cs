using Lumen.Companion;
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
        }

        private void OnDisable()
        {
            if (commandIssuer != null)
            {
                commandIssuer.Feedback -= ShowFeedback;
            }
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
