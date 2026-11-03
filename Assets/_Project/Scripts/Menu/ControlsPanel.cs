using UnityEngine;
using UnityEngine.UI;

namespace Lumen.Menu
{
    /// <summary>
    /// "Tuşlar" paneli: ana menüde ve duraklatma menüsünde aynı içerikle açılır.
    /// Oyun ekranında yalnızca F ipucu kalır, diğer tuşlar burada listelenir.
    /// </summary>
    public class ControlsPanel : MonoBehaviour
    {
        /// <summary>Tüm tuşların tek kaynağı: (tuş, açıklama). Panel bunları iki sütun hâlinde yazar.</summary>
        public static readonly (string key, string action)[] Keys =
        {
            ("WASD", "Hareket"),
            ("Fare", "Etrafa bak"),
            ("E", "Etkileşim (yazıt, kristal, Arven)"),
            ("F", "Lumi'den yardım iste"),
            ("G", "Lumi beni takip etsin"),
            ("Enter", "Arven'e mesaj gönder"),
            ("Tab", "Sohbeti kapat"),
            ("Esc", "Menü")
        };

        [SerializeField] private Button openButton;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button backButton;

        public bool IsOpen => panel.activeSelf;

        public void Configure(Button open, GameObject controlsPanel, Button back)
        {
            openButton = open;
            panel = controlsPanel;
            backButton = back;
        }

        private void Awake()
        {
            openButton.onClick.AddListener(() => panel.SetActive(true));
            backButton.onClick.AddListener(() => panel.SetActive(false));
            panel.SetActive(false);
        }

        private void Update()
        {
            if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                panel.SetActive(false);
            }
        }

        // Menü kapanırsa panel de kapanır; menü yeniden açılınca panel açık kalmış olmasın
        private void OnDisable()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
