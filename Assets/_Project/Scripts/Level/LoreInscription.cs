using System;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Duvardaki yazıt. Oyuncu E ile okuduğunda metni LoreJournal'a ekler ve Read olayını tetikler;
    /// metni ekranda göstermek arayüzün işidir. Tekrar okunabilir, günlüğe yalnızca bir kez yazılır.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LoreInscription : MonoBehaviour, IInteractable
    {
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 8)] private string text;
        [SerializeField] private LoreJournal journal;
        [SerializeField] private Light glyphLight;
        [SerializeField] private Color readColor = new Color(0.55f, 0.6f, 0.7f);

        public event Action<LoreInscription> Read;

        public string Title => title;

        public string Text => text;

        public bool HasBeenRead { get; private set; }

        public string Prompt => HasBeenRead ? "[E] Tekrar oku" : "[E] Oku";

        public void Configure(string inscriptionTitle, string inscriptionText, LoreJournal loreJournal, Light glyph)
        {
            title = inscriptionTitle;
            text = inscriptionText;
            journal = loreJournal;
            glyphLight = glyph;
        }

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrEmpty(text);
        }

        public void Interact(GameObject interactor)
        {
            if (!HasBeenRead)
            {
                HasBeenRead = true;
                journal?.Add($"{title}: {text}");

                if (glyphLight != null)
                {
                    glyphLight.color = readColor;
                }
            }

            Read?.Invoke(this);
        }
    }
}
