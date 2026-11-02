using Lumen.Core;
using UnityEngine;

namespace Lumen.NPC
{
    // Kapının önündeki taş bekçi. Oyuncu yaklaşınca "[E] Arven ile konuş" görünür (PlayerInteractor bulur),
    // E'ye basınca sohbet paneli açılır. Yazıtlarla aynı IInteractable arayüzünü kullanır.
    [RequireComponent(typeof(Collider))]
    public sealed class ArvenNpc : MonoBehaviour, IInteractable
    {
        [SerializeField] private ChatView chat;
        [SerializeField] private GameStateManager gameState;

        public string Prompt => "[E] Arven ile konuş";

        public void Configure(ChatView view, GameStateManager state)
        {
            chat = view;
            gameState = state;
        }

        public bool CanInteract(GameObject interactor) => !chat.IsOpen && !gameState.IsGameOver;

        public void Interact(GameObject interactor) => chat.Open();
    }
}
