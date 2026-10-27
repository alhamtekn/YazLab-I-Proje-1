using UnityEngine;

namespace Lumen.Core
{
    /// <summary>
    /// Oyuncunun yakınına gelip etkileşim tuşuyla kullanabildiği nesneler (NPC, yazıt vb.).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Ekranda gösterilecek kısa ipucu, örn. "[E] Konuş".</summary>
        string Prompt { get; }

        bool CanInteract(GameObject interactor);

        void Interact(GameObject interactor);
    }
}
