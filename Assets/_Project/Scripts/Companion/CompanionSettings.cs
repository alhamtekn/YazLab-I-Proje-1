using System;
using UnityEngine;

namespace Lumen.Companion
{
    /// <summary>
    /// Durum makinesinin eşik değerleri. Inspector'dan ayarlanabilir.
    /// </summary>
    [Serializable]
    public class CompanionSettings
    {
        [Tooltip("Oyuncu bu mesafeden uzaklaşınca Idle -> Follow.")]
        public float followStartDistance = 3.5f;

        [Tooltip("Oyuncuya bu mesafeye kadar yaklaşınca Follow -> Idle.")]
        public float followStopDistance = 2f;

        [Tooltip("Hedefin yaklaşma noktasına bu yatay mesafede ulaşılmış sayılır.")]
        public float arriveDistance = 0.45f;

        [Tooltip("Gidiş yönündeki tehlikeli okuma bu mesafeden yakınsa Avoid durumuna geçilir.")]
        public float avoidDistance = 1.4f;

        [Tooltip("Avoid'den çıkmak için tehlikenin uzaklaşması gereken ek pay (titremeyi önler).")]
        public float avoidExitMargin = 0.4f;

        [Tooltip("Gidiş yönüne göre tehlike konisinin yarım açısı (derece).")]
        public float avoidAngle = 30f;

        [Tooltip("Avoid durumunda en fazla bekleme süresi; sonra rota yeniden denenir.")]
        public float avoidTimeout = 6f;

        [Tooltip("NavMesh yolunun yeniden hesaplanma aralığı (saniye).")]
        public float repathInterval = 0.4f;

        [Tooltip("Hedefi tetiklemek için harcanan süre (saniye).")]
        public float interactDuration = 0.8f;
    }
}
