using Lumen.Core;
using Lumen.Level;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.Player
{
    /// <summary>
    /// Okunan yazıtı ekranın ortasında gösteren panel. Açıkken oyuncu girdisi kilitlenir;
    /// E, Boşluk ya da fare tıklamasıyla kapanır. Esc duraklatma menüsüne bırakılmıştır.
    /// </summary>
    public class LoreReaderView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private LoreInscription[] inscriptions = System.Array.Empty<LoreInscription>();

        private int openedFrame;
        private bool releasePending;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Configure(GameObject readerPanel, Text title, Text body, LoreInscription[] sources)
        {
            panel = readerPanel;
            titleText = title;
            bodyText = body;
            inscriptions = sources;
        }

        public void Show(LoreInscription inscription)
        {
            titleText.text = inscription.Title;
            bodyText.text = inscription.Text;
            panel.SetActive(true);
            openedFrame = Time.frameCount;
            InputLock.Acquire(this);
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            panel.SetActive(false);
            releasePending = true;
        }

        private void Awake()
        {
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            foreach (LoreInscription inscription in inscriptions)
            {
                if (inscription != null)
                {
                    inscription.Read += Show;
                }
            }
        }

        private void OnDisable()
        {
            foreach (LoreInscription inscription in inscriptions)
            {
                if (inscription != null)
                {
                    inscription.Read -= Show;
                }
            }

            Close();
            ReleaseInput();
        }

        // Kilit karenin sonunda bırakılır; böylece paneli kapatan E aynı karede yazıtı yeniden açmaz.
        private void LateUpdate()
        {
            if (releasePending)
            {
                ReleaseInput();
            }
        }

        private void ReleaseInput()
        {
            releasePending = false;
            InputLock.Release(this);
        }

        private void Update()
        {
            // Paneli açan E basışı aynı karede kapatmasın.
            if (!IsOpen || Time.frameCount == openedFrame)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                Close();
            }
        }
    }
}
