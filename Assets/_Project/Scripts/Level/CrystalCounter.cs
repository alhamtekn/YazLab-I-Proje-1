using System;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Ara bölgedeki kristalleri izler. Her toplamada CountChanged, gereken sayıya
    /// ulaşıldığında bir kez AllCollected olayını tetikler.
    /// </summary>
    public class CrystalCounter : MonoBehaviour
    {
        [SerializeField] private CrystalPickup[] crystals = Array.Empty<CrystalPickup>();
        [SerializeField] private int required = 3;

        public event Action<int, int> CountChanged;

        public event Action AllCollected;

        public int Collected { get; private set; }

        public int Required => required;

        public bool IsComplete => Collected >= required;

        public void Configure(CrystalPickup[] trackedCrystals, int requiredCount)
        {
            Unsubscribe();
            crystals = trackedCrystals;
            required = requiredCount;
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
                AllCollected?.Invoke();
            }
        }
    }
}
