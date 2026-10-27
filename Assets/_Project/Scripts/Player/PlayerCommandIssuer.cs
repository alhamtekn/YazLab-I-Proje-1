using System;
using Lumen.Companion;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Player
{
    /// <summary>
    /// Oyuncunun eşlikçiye emir vermesi: F = yardım et, G = beni takip et.
    /// Eşlikçinin somut sınıfını bilmez; yalnızca ICompanionCommandReceiver ile konuşur.
    /// </summary>
    public class PlayerCommandIssuer : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour companion;
        [SerializeField] private KeyCode helpKey = KeyCode.F;
        [SerializeField] private KeyCode followKey = KeyCode.G;

        private ICompanionCommandReceiver receiver;

        /// <summary>HUD'da gösterilecek geri bildirim (örn. "Lumi: yardım edilecek bir şey yok").</summary>
        public event Action<string> Feedback;

        public void Configure(MonoBehaviour companionReceiver)
        {
            companion = companionReceiver;
        }

        private void Awake()
        {
            receiver = companion as ICompanionCommandReceiver;
            if (receiver == null)
            {
                Debug.LogError($"{name}: atanmış eşlikçi ICompanionCommandReceiver uygulamıyor.", this);
            }
        }

        private void Update()
        {
            if (receiver == null || InputLock.IsLocked)
            {
                return;
            }

            if (Input.GetKeyDown(helpKey))
            {
                Feedback?.Invoke(receiver.CommandHelp()
                    ? "Lumi: Hemen hallediyorum!"
                    : "Lumi: Şu an yardım edebileceğim bir şey görmüyorum.");
            }
            else if (Input.GetKeyDown(followKey))
            {
                receiver.CommandFollow();
                Feedback?.Invoke("Lumi: Peşindeyim.");
            }
        }
    }
}
