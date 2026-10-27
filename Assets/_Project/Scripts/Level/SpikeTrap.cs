using Lumen.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.Level
{
    public enum SpikePhase
    {
        Down,
        Warning,
        Up
    }

    /// <summary>
    /// Kapalı -> uyarı (kırmızı parlama) -> açık döngüsüyle çalışan diken tuzağı.
    /// Süreler tohumlu rastgele sayıyla seçilir; aynı tohum aynı düzeni verir (tekrarlanabilir test).
    /// Açıkken NavMeshObstacle ile NavMesh'i oyar, böylece ajanın yolu tuzağın etrafından hesaplanır.
    /// </summary>
    public class SpikeTrap : MonoBehaviour, IHazard
    {
        [SerializeField] private Transform spikes;
        [SerializeField] private Renderer warningRenderer;
        [SerializeField] private NavMeshObstacle obstacle;
        [SerializeField] private Vector2 downDuration = new Vector2(1.6f, 3.2f);
        [SerializeField] private float warningDuration = 0.6f;
        [SerializeField] private Vector2 upDuration = new Vector2(1.2f, 2.4f);
        [SerializeField] private float spikeHeight = 0.5f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private System.Random random = new System.Random();
        private MaterialPropertyBlock block;
        private Vector3 resetPosition;
        private float phaseTimer;

        public SpikePhase Phase { get; private set; } = SpikePhase.Down;

        public bool IsDangerous => Phase != SpikePhase.Down;

        public void Configure(Transform spikeVisual, Renderer warning, NavMeshObstacle navObstacle)
        {
            spikes = spikeVisual;
            warningRenderer = warning;
            obstacle = navObstacle;
        }

        /// <summary>Tuzak alanı tarafından çağrılır: tohum, başlangıç fazı ve yakalanınca dönülecek nokta.</summary>
        public void Initialize(int seed, Vector3 victimResetPosition)
        {
            random = new System.Random(seed);
            resetPosition = victimResetPosition;
            SetPhase(SpikePhase.Down);
            phaseTimer = Range(0f, downDuration.y);
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            ApplyVisuals();
        }

        private void Update()
        {
            phaseTimer -= Time.deltaTime;
            if (phaseTimer > 0f)
            {
                return;
            }

            switch (Phase)
            {
                case SpikePhase.Down:
                    SetPhase(SpikePhase.Warning);
                    phaseTimer = warningDuration;
                    break;
                case SpikePhase.Warning:
                    SetPhase(SpikePhase.Up);
                    phaseTimer = Range(upDuration.x, upDuration.y);
                    break;
                default:
                    SetPhase(SpikePhase.Down);
                    phaseTimer = Range(downDuration.x, downDuration.y);
                    break;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (Phase != SpikePhase.Up)
            {
                return;
            }

            other.GetComponentInParent<IHazardVictim>()?.OnHazardHit(resetPosition);
        }

        private void SetPhase(SpikePhase phase)
        {
            Phase = phase;
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (obstacle != null)
            {
                obstacle.enabled = Phase == SpikePhase.Up;
            }

            if (spikes != null)
            {
                Vector3 p = spikes.localPosition;
                p.y = Phase == SpikePhase.Up ? spikeHeight * 0.5f : -spikeHeight * 0.45f;
                spikes.localPosition = p;
            }

            if (warningRenderer != null && block != null)
            {
                Color glow = Phase switch
                {
                    SpikePhase.Warning => new Color(2.5f, 0.6f, 0.1f),
                    SpikePhase.Up => new Color(3f, 0.1f, 0.05f),
                    _ => new Color(0.05f, 0.05f, 0.05f)
                };
                warningRenderer.GetPropertyBlock(block);
                block.SetColor(EmissionColorId, glow);
                warningRenderer.SetPropertyBlock(block);
            }
        }

        private float Range(float min, float max) => min + (float)random.NextDouble() * (max - min);
    }
}
