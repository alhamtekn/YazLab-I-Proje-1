using System;
using System.Collections;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Ara bölgedeki toplanabilir kristal. Kendi ekseninde döner, ışığı yavaşça nabız atar.
    /// Oyuncu üzerinden geçince ya da yanındayken E'ye basınca toplanır ve Collected olayını tetikler.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CrystalPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform visual;
        [SerializeField] private Light glow;
        [SerializeField] private float spinSpeed = 60f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float collectDuration = 0.35f;

        private Vector3 visualBasePosition;
        private float baseIntensity;

        public event Action<CrystalPickup> Collected;

        public bool IsCollected { get; private set; }

        public string Prompt => "[E] Al";

        public void Configure(Transform crystalVisual, Light crystalGlow)
        {
            visual = crystalVisual;
            glow = crystalGlow;
        }

        public bool CanInteract(GameObject interactor)
        {
            return !IsCollected;
        }

        public void Interact(GameObject interactor)
        {
            Collect();
        }

        public void Collect()
        {
            if (IsCollected)
            {
                return;
            }

            IsCollected = true;
            GetComponent<Collider>().enabled = false;
            StartCoroutine(Vanish());
            Collected?.Invoke(this);
        }

        private void Awake()
        {
            if (visual != null)
            {
                visualBasePosition = visual.localPosition;
            }

            if (glow != null)
            {
                baseIntensity = glow.intensity;
            }
        }

        private void Update()
        {
            if (IsCollected)
            {
                return;
            }

            if (visual != null)
            {
                visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
                visual.localPosition = visualBasePosition + Vector3.up * (Mathf.Sin(Time.time * pulseSpeed) * bobHeight);
            }

            if (glow != null)
            {
                glow.intensity = baseIntensity * (0.75f + 0.25f * Mathf.Sin(Time.time * pulseSpeed * 1.5f));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == GameLayers.PlayerIndex)
            {
                Collect();
            }
        }

        private IEnumerator Vanish()
        {
            Vector3 fromScale = visual != null ? visual.localScale : Vector3.one;
            float fromIntensity = glow != null ? glow.intensity : 0f;

            for (float t = 0f; t < 1f; t += Time.deltaTime / collectDuration)
            {
                if (visual != null)
                {
                    visual.localScale = Vector3.Lerp(fromScale, Vector3.zero, t);
                    visual.localPosition += Vector3.up * (Time.deltaTime * 2f);
                }

                if (glow != null)
                {
                    glow.intensity = Mathf.Lerp(fromIntensity * 2f, 0f, t);
                }

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
