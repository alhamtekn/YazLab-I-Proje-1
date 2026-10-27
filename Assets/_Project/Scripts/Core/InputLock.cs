using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lumen.Core
{
    /// <summary>
    /// Oyuncu girdisini geçici olarak kilitleyen sahipli kilit.
    /// Örn. sohbet arayüzü açıkken hareket ve kamera durur, imleç serbest kalır.
    /// Birden fazla sahip aynı anda kilitleyebilir; son sahip bırakınca kilit kalkar.
    /// </summary>
    public static class InputLock
    {
        private static readonly HashSet<object> Owners = new HashSet<object>();

        public static bool IsLocked => Owners.Count > 0;

        public static event Action<bool> Changed;

        public static void Acquire(object owner)
        {
            if (Owners.Add(owner) && Owners.Count == 1)
            {
                Changed?.Invoke(true);
            }
        }

        public static void Release(object owner)
        {
            if (Owners.Remove(owner) && Owners.Count == 0)
            {
                Changed?.Invoke(false);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Owners.Clear();
            Changed = null;
        }
    }
}
