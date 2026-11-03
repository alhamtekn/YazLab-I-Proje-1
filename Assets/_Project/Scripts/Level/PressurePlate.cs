using System;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Diken odasındaki oyuncu plakası: üzerinde durulurken bağlı tuzaklar iner ve durur; plakadan
    /// inildikten sonra holdSeconds kadar daha inik kalır, oyuncu bu sürede karşıya geçmelidir.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] private SpikeTrap[] traps = Array.Empty<SpikeTrap>();
        [SerializeField] private Light indicator;
        [SerializeField] private float holdSeconds = 6f;

        private int occupants;
        private float releaseTimer;

        public bool IsHolding => occupants > 0 || releaseTimer > 0f;

        public void Configure(SpikeTrap[] linkedTraps, Light statusLight, float secondsAfterLeaving)
        {
            traps = linkedTraps;
            indicator = statusLight;
            holdSeconds = secondsAfterLeaving;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != GameLayers.PlayerIndex)
            {
                return;
            }

            occupants++;
            SetTraps(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != GameLayers.PlayerIndex)
            {
                return;
            }

            occupants = Mathf.Max(0, occupants - 1);
            if (occupants == 0)
            {
                releaseTimer = holdSeconds;
            }
        }

        private void Update()
        {
            if (occupants == 0 && releaseTimer > 0f)
            {
                releaseTimer -= Time.deltaTime;
                if (releaseTimer <= 0f)
                {
                    SetTraps(false);
                }
            }

            if (indicator != null)
            {
                indicator.intensity = IsHolding ? 1.6f + 0.6f * Mathf.Sin(Time.time * 8f) : 0.4f;
            }
        }

        private void SetTraps(bool suppressed)
        {
            foreach (SpikeTrap trap in traps)
            {
                trap.SetSuppressed(suppressed);
            }
        }
    }
}
