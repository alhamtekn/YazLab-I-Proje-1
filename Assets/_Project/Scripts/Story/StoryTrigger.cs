using Lumen.Core;
using UnityEngine;

namespace Lumen.Story
{
    /// <summary>Oyuncu içinden ilk geçtiğinde hikâye metnini gösteren görünmez tetik (bir kez çalışır).</summary>
    [RequireComponent(typeof(Collider))]
    public class StoryTrigger : MonoBehaviour
    {
        [SerializeField] private StoryView view;
        [SerializeField, TextArea] private string message;

        public bool Played { get; private set; }

        public void Configure(StoryView storyView, string text)
        {
            view = storyView;
            message = text;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Played || other.gameObject.layer != GameLayers.PlayerIndex)
            {
                return;
            }

            Played = true;
            view.Show(message);
        }
    }
}
