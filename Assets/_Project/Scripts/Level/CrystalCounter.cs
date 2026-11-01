using System;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Ara bölgedeki kristalleri izler. Her toplamada CountChanged, gereken sayıya
    /// ulaşıldığında bir kez AllCollected olayını tetikler ve bağlı IActivatable nesneleri (örn. kapı) açar.
    /// </summary>
    public class CrystalCounter : MonoBehaviour
    {
        [SerializeField] private CrystalPickup[] crystals = Array.Empty<CrystalPickup>();
        [SerializeField] private int required = 3;
        [SerializeField] private MonoBehaviour[] linkedActivatables = Array.Empty<MonoBehaviour>();

        public event Action<int, int> CountChanged;

        public event Action AllCollected;

        public int Collected { get; private set; }

        public int Required => required;

        public bool IsComplete => Collected >= required;

        public void Configure(CrystalPickup[] trackedCrystals, int requiredCount, MonoBehaviour[] linked)
        {
            Unsubscribe();
            crystals = trackedCrystals;
            required = requiredCount;
            linkedActivatables = linked;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            foreach (CrystalPickup crystal in crystals)
            {
                if (crystal != null)
                {
                    crystal.Collected += OnCrystalCollected;
                }
            }
        }

        private void Unsubscribe()
        {
            foreach (CrystalPickup crystal in crystals)
            {
                if (crystal != null)
                {
                    crystal.Collected -= OnCrystalCollected;
                }
            }
        }

        private void OnCrystalCollected(CrystalPickup crystal)
        {
            if (IsComplete)
            {
                return;
            }

            Collected++;
            CountChanged?.Invoke(Collected, required);

            if (IsComplete)
            {
                foreach (MonoBehaviour behaviour in linkedActivatables)
                {
                    (behaviour as IActivatable)?.Activate();
                }

                AllCollected?.Invoke();
            }
        }
    }
}
