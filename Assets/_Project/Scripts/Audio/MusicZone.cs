using UnityEngine;

namespace Lumen.Audio
{
    /// <summary>
    /// Oyuncu kutsal bölüme girince (z eşiği) müzik yavaşça daha gergin parçaya geçer, geri dönünce eskisine döner.
    /// İki kaynak hep çalar; yalnızca ses düzeyleri çapraz geçişle değişir, böylece parça baştan başlamaz.
    /// </summary>
    public class MusicZone : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private AudioSource normal;
        [SerializeField] private AudioSource tense;
        [SerializeField] private float thresholdZ;
        [SerializeField] private float volume = 0.45f;
        [SerializeField] private float fadeSeconds = 3f;

        /// <summary>0: normal müzik, 1: gergin müzik.</summary>
        public float Blend { get; private set; }

        public void Configure(Transform playerTransform, AudioSource normalMusic, AudioSource tenseMusic, float zoneStartZ)
        {
            player = playerTransform;
            normal = normalMusic;
            tense = tenseMusic;
            thresholdZ = zoneStartZ;
        }

        private void Update()
        {
            float target = player.position.z > thresholdZ ? 1f : 0f;
            Blend = Mathf.MoveTowards(Blend, target, Time.unscaledDeltaTime / fadeSeconds);
            normal.volume = volume * (1f - Blend);
            tense.volume = volume * Blend;
        }
    }
}
