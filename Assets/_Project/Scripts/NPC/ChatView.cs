using System;
using Lumen.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.NPC
{
    // Arven ile sohbet paneli. Yalnızca ekranı yönetir ve oyuncunun yazdığını haber verir;
    // modele soru sorma işi ArvenDialogue'dadır (her sınıfın tek sorumluluğu var).
    public sealed class ChatView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text historyText;   // son konuşma: oyuncunun sözü + Arven'in cevabı
        [SerializeField] private Text thinkingText;  // "Arven düşünüyor..." göstergesi
        [SerializeField] private InputField input;

        private bool thinking;

        // Oyuncu Enter'a bastığında yazdığı metinle tetiklenir
        public event Action<string> Submitted;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Configure(GameObject chatPanel, Text history, Text thinkingLabel, InputField field)
        {
            panel = chatPanel;
            historyText = history;
            thinkingText = thinkingLabel;
            input = field;
        }

        public void Open()
        {
            panel.SetActive(true);
            InputLock.Acquire(this); // yazarken karakter yürümesin, kamera dönmesin; imleç serbest kalır
            Focus();
        }

        public void Close()
        {
            panel.SetActive(false);
            InputLock.Release(this);
        }

        // Cevap beklenirken giriş kapanır, gösterge açılır; cevap gelince tersi
        public void SetThinking(bool value)
        {
            thinking = value;
            thinkingText.gameObject.SetActive(value);
            input.interactable = !value;
            if (!value && IsOpen)
            {
                Focus();
            }
        }

        public void ShowReply(NPCResponse response)
        {
            historyText.text += $"\nArven ({MoodLabel(response.mood)}): {response.dialogue}";
        }

        private void Awake()
        {
            panel.SetActive(false);
            thinkingText.gameObject.SetActive(false);
            historyText.text = string.Empty;
        }

        private void OnDestroy()
        {
            InputLock.Release(this);
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Close();
                return;
            }

            if (thinking)
            {
                // Noktalar oynar: oyun donmadan cevap beklendiğini gösterir (async çağrının kanıtı)
                int dots = 1 + (int)(Time.unscaledTime * 3f) % 3;
                thinkingText.text = "Arven düşünüyor" + new string('.', dots);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                string message = input.text.Trim();
                input.text = string.Empty;
                if (message.Length > 0)
                {
                    historyText.text = "Sen: " + message;
                    Submitted?.Invoke(message);
                }
                Focus();
            }
        }

        // Yazı kutusunu seçer ki oyuncu tıklamadan yazmaya başlayabilsin
        private void Focus()
        {
            input.Select();
            input.ActivateInputField();
        }

        private static string MoodLabel(string mood) => mood switch
        {
            "friendly" => "dostça",
            "suspicious" => "şüpheli",
            "angry" => "öfkeli",
            _ => "sakin"
        };
    }
}
