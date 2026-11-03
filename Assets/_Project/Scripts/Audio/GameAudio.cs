using Lumen.Level;
using Lumen.NPC;
using Lumen.Sanctum;
using UnityEngine;

namespace Lumen.Audio
{
    /// <summary>
    /// Oyun olaylarına kısa ses efekti bağlar; olayların sahibi olan sınıflara dokunmadan yalnızca olaylarını dinler.
    /// Çan: kristal toplama, yazıt okuma, şalter, Lumi işareti. Gümbürtü: kapılar ve bariyer açılınca. Darbe: Arven saldırınca.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class GameAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip chime;
        [SerializeField] private AudioClip rumble;
        [SerializeField] private AudioClip hit;
        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

        private AudioSource source;

        public void Configure(AudioClip chimeClip, AudioClip rumbleClip, AudioClip hitClip)
        {
            chime = chimeClip;
            rumble = rumbleClip;
            hit = hitClip;
        }

        public void PlayChime() => source.PlayOneShot(chime, volume);

        public void PlayRumble() => source.PlayOneShot(rumble, volume);

        public void PlayHit() => source.PlayOneShot(hit, volume);

        private void Awake() => source = GetComponent<AudioSource>();

        // Start'ta sahnedeki tüm nesneler hazırdır; olayları burada dinlemeye başlarız
        private void Start()
        {
            foreach (CrystalCounter counter in FindObjectsByType<CrystalCounter>()) counter.CountChanged += (_, _) => PlayChime();
            foreach (LoreInscription inscription in FindObjectsByType<LoreInscription>()) inscription.Read += _ => PlayChime();
            foreach (SwitchMechanism mechanism in FindObjectsByType<SwitchMechanism>()) mechanism.Activated += PlayChime;
            foreach (CompanionBeacon beacon in FindObjectsByType<CompanionBeacon>()) beacon.Lit += PlayChime;

            foreach (CrystalGate gate in FindObjectsByType<CrystalGate>()) gate.Activated += PlayRumble;
            foreach (Drawbridge bridge in FindObjectsByType<Drawbridge>()) bridge.Activated += PlayRumble;
            foreach (MazeGate gate in FindObjectsByType<MazeGate>()) gate.Activated += PlayRumble;
            foreach (ArvenDoor door in FindObjectsByType<ArvenDoor>()) door.Activated += PlayRumble;
            foreach (TwinSeal seal in FindObjectsByType<TwinSeal>()) seal.Opened += PlayRumble;

            foreach (GameStateManager state in FindObjectsByType<GameStateManager>()) state.PlayerAttacked += PlayHit;
        }
    }
}
