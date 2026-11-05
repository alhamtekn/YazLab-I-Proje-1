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
        [SerializeField] private Text trustText;     // "Güven: 15 / 30"
        [SerializeField] private RectTransform trustFill; // güven çubuğunun dolan kısmı (genişliği güvenle orantılı)
        [SerializeField] private GameStateManager gameState;
        [SerializeField] private AudioSource replySound;     // Arven cevap verince yumuşak çan
        [SerializeField] private float charactersPerSecond = 45f;

        private bool thinking;
        private string fullText = string.Empty; // panelde sonunda görünecek metnin tamamı
        private int replyEnd;                   // harf harf yazılan cevabın bittiği yer
        private float revealed;                 // şu ana kadar yazılan karakter sayısı

        /// <summary>Arven'in cevabı hâlâ harf harf yazılıyor mu.</summary>
        public bool IsTyping { get; private set; }

        public string HistoryText => historyText.text;

        // Oyuncu Enter'a bastığında yazdığı metinle tetiklenir
        public event Action<string> Submitted;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Configure(GameObject chatPanel, Text history, Text thinkingLabel, InputField field, Text trust, RectTransform trustBarFill, GameStateManager state, AudioSource sound)
        {
            replySound = sound;
            panel = chatPanel;
            historyText = history;
            thinkingText = thinkingLabel;
            input = field;
            trustText = trust;
            trustFill = trustBarFill;
            gameState = state;
        }

        /// <summary>Güven çubuğunun doluluk oranı, 0-1 (test için).</summary>
        public float TrustBarFill => trustFill.anchorMax.x;

        public void Open()
        {
            panel.SetActive(true);
            ShowTrust(gameState.Trust); // ilerleme açılışta güncel görünsün
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

        // Cevap harf harf yazılır (Update); tam metin hemen hazırlanır, böylece arkasından gelen bildirim sıraya girer
        public void ShowReply(NPCResponse response)
        {
            revealed = fullText.Length;
            fullText += $"\nArven ({MoodLabel(response.mood)}): {response.dialogue}";
            replyEnd = fullText.Length;
            IsTyping = true;
            replySound.Play();
        }

        private void ShowTrust(int trust)
        {
            string crystals = gameState.CrystalsComplete ? "tamam" : "eksik";
            trustText.text = $"Güven: {trust} / {gameState.TrustThreshold}\n" +
                             $"Yazıt {gameState.LoreRead}/{gameState.RequiredLore} · Kristal {crystals}";

            // Çubuk: eşiğe göre doluluk; renk turuncudan (az) yeşile (eşik) döner, eksi güvende boş kalır
            float fill = Mathf.Clamp01((float)trust / gameState.TrustThreshold);
            trustFill.anchorMax = new Vector2(fill, 1f);
            trustFill.GetComponent<Image>().color = Color.Lerp(new Color(1f, 0.55f, 0.2f), new Color(0.4f, 1f, 0.5f), fill);
        }

        // Oyun durumundan gelen bilgi (kapı, saldırı) Arven'in cevabının altına eklenir; cevap yazılıyorsa bitince görünür
        private void ShowNotice(string message)
        {
            fullText += "\n<i>" + message + "</i>";
            if (!IsTyping)
            {
                historyText.text = fullText;
            }
        }

        // Cevabı biraz daha yaz; cevap bitince (bildirim etiketleri yarım görünmesin diye) metnin tamamını göster
        private void TypeReply()
        {
            revealed += charactersPerSecond * Time.unscaledDeltaTime;
            if (revealed >= replyEnd)
            {
                IsTyping = false;
                historyText.text = fullText;
                return;
            }

            historyText.text = fullText.Substring(0, (int)revealed);
        }

        private void Awake()
        {
            panel.SetActive(false);
            thinkingText.gameObject.SetActive(false);
            historyText.text = string.Empty;
            ShowTrust(gameState.Trust);
        }

        // Panel oyun durumunu yalnızca dinler; kararları GameStateManager verir
        private void OnEnable()
        {
            gameState.TrustChanged += ShowTrust;
            gameState.Notice += ShowNotice;
            gameState.PlayerAttacked += Close;
        }

        private void OnDisable()
        {
            gameState.TrustChanged -= ShowTrust;
            gameState.Notice -= ShowNotice;
            gameState.PlayerAttacked -= Close;
        }

        private void OnDestroy()
        {
            InputLock.Release(this);
        }

        private void Update()
        {
            if (IsTyping)
            {
                TypeReply();
            }

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
                    fullText = "Sen: " + message;
                    historyText.text = fullText;
                    IsTyping = false;
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
