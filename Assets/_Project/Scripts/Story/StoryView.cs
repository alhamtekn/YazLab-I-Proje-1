using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.Story
{
    /// <summary>
    /// Hikâye anlatım metni: ekranın alt ortasında yavaşça belirir, okunacak kadar kalır, yavaşça kaybolur.
    /// Peş peşe gelen metinler sıraya girer, üst üste binmez.
    /// </summary>
    public class StoryView : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float fadeSeconds = 1f;
        [SerializeField] private float secondsPerCharacter = 0.06f; // uzun metin daha uzun kalır
        [SerializeField] private float minHoldSeconds = 3.5f;

        private readonly Queue<string> queue = new Queue<string>();
        private bool playing;

        /// <summary>Şu an gösterilen ya da sırada bekleyen metin var mı (test için).</summary>
        public bool IsBusy => playing || queue.Count > 0;

        public string CurrentText => text.text;

        public void Configure(Text storyText, CanvasGroup canvasGroup)
        {
            text = storyText;
            group = canvasGroup;
        }

        public void Show(string message)
        {
            queue.Enqueue(message);
            if (!playing)
            {
                StartCoroutine(Play());
            }
        }

        private void Awake() => group.alpha = 0f;

        private IEnumerator Play()
        {
            playing = true;
            while (queue.Count > 0)
            {
                text.text = queue.Dequeue();
                yield return Fade(0f, 1f);
                yield return new WaitForSeconds(Mathf.Max(minHoldSeconds, text.text.Length * secondsPerCharacter));
                yield return Fade(1f, 0f);
            }

            playing = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / fadeSeconds)
            {
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            group.alpha = to;
        }
    }
}
