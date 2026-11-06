using System;
using System.Collections;
using Lumen.Core;
using Lumen.Level;
using UnityEngine;

namespace Lumen.NPC
{
    // Oyunun durumu: Arven'in güveni, kapının açılması, saldırı ve oyunun sonu.
    // Kapıyı açma kararı yalnızca modele bırakılmaz (prompt injection'a karşı):
    //  - tek mesajda güven en fazla maxTrustGain kadar artar,
    //  - bölümler (3 kristal + 3 yazıt) bitmeden güven eşiğe ulaşamaz,
    //  - model open_door dese bile güven eşiğin altındaysa kapı açılmaz; güven eşiğe ulaşınca kapı kendiliğinden açılır.
    public sealed class GameStateManager : MonoBehaviour, IGameOver
    {
        [SerializeField] private ArvenDialogue dialogue;
        [SerializeField] private MonoBehaviour door;    // IActivatable: Arven'in kapısı
        [SerializeField] private MonoBehaviour player;  // IRespawnable: saldırıda oyuncu son kontrol noktasına atılır
        [SerializeField] private MonoBehaviour journal; // ILoreJournal: okunan yazıtlar (Onur'un ara bölgesi)
        [SerializeField] private CrystalCounter crystals;
        [SerializeField] private int trustThreshold = 30;
        [SerializeField] private int maxTrustGain = 10;  // model +20 dese bile tek mesajda en fazla bu kadar artar
        [SerializeField] private int requiredLore = 3;   // okunması gereken yazıt sayısı
        [SerializeField] private float attackDelay = 2f; // oyuncu Arven'in sözünü okuyabilsin diye bekleme

        private IActivatable doorActivatable;
        private IRespawnable playerRespawn;
        private ILoreJournal loreJournal;

        public int Trust { get; private set; }
        public int TrustThreshold => trustThreshold;
        public bool IsGameOver { get; private set; }
        public bool IsLost { get; private set; }

        // İlerleme: eksik bölüm yoksa (sahnede kurulmamışsa) tamam sayılır
        public int LoreRead => loreJournal != null ? loreJournal.Entries.Count : requiredLore;
        public int RequiredLore => requiredLore;
        public bool CrystalsComplete => crystals == null || crystals.IsComplete;
        public bool ProgressComplete => CrystalsComplete && LoreRead >= requiredLore;

        public event Action<int> TrustChanged;  // güven değişti (panel gösterir)
        public event Action<string> Notice;     // oyuncuya kısa bilgi: kapı açıldı / açılmadı / saldırı
        public event Action PlayerAttacked;     // oyuncu geri atıldı (panel kapanır)
        public event Action GameEnded;
        public event Action<string> GameLost;   // oyuncu kaybetti (sebep): "Şansını kaybettin" ekranı açılır

        public void Configure(ArvenDialogue arvenDialogue, MonoBehaviour arvenDoor, MonoBehaviour playerRespawnable,
            MonoBehaviour loreJournalSource, CrystalCounter crystalCounter)
        {
            dialogue = arvenDialogue;
            door = arvenDoor;
            player = playerRespawnable;
            journal = loreJournalSource;
            crystals = crystalCounter;
        }

        // Kapı NPC bölümünde kurulur (ChatSection'dan sonra), bu yüzden ayrıca bağlanır
        public void SetDoor(MonoBehaviour arvenDoor) => door = arvenDoor;

        public void EndGame()
        {
            if (IsGameOver || IsLost)
            {
                return;
            }

            IsGameOver = true;
            GameEnded?.Invoke();
        }

        // Lav, Lumi'nin parkuru ya da Arven: oyun bittiyse veya zaten kaybedildiyse bir şey yapmaz
        public void Lose(string reason)
        {
            if (IsGameOver || IsLost)
            {
                return;
            }

            IsLost = true;
            Debug.Log("[Oyun] Kaybedildi: " + reason);
            GameLost?.Invoke(reason);
        }

        private void Awake()
        {
            doorActivatable = door as IActivatable;
            playerRespawn = player as IRespawnable;
            loreJournal = journal as ILoreJournal;
        }

        private void OnEnable() => dialogue.Responded += OnResponded;

        private void OnDisable() => dialogue.Responded -= OnResponded;

        // Güvenin çıkabileceği en yüksek değer: bölümler bitmeden eşiğin bir altı
        private int MaxTrust() => ProgressComplete ? 100 : trustThreshold - 1;

        // Arven'in her (ayrıştırılmış) cevabında çalışır
        private void OnResponded(NPCResponse response)
        {
            if (IsGameOver || IsLost)
            {
                return;
            }

            int delta = Mathf.Min(response.trustDelta, maxTrustGain); // düşüş sınırsız, artış sınırlı
            int wanted = Trust + delta;
            Trust = Mathf.Clamp(wanted, -100, MaxTrust());
            TrustChanged?.Invoke(Trust);

            if (wanted > Trust && !ProgressComplete)
            {
                Notice?.Invoke("Arven sana tam güvenmiyor: önce kristalleri topla ve tapınağın yazıtlarını oku.");
            }

            Debug.Log($"[Arven] güven {Trust}/{trustThreshold} (model: {response.trustDelta}), eylem {response.action}, " +
                      $"yazıt {LoreRead}/{requiredLore}, kristal {(CrystalsComplete ? "tamam" : "eksik")} | {response.reason}");

            // Model güven puanını bilmez: güven eşiğe ulaştıysa kapıyı oyun kuralı açar, modelin ayrıca istemesi gerekmez
            if (response.action == "open_door" || Trust >= trustThreshold)
            {
                TryOpenDoor();
            }
            else if (response.action == "attack")
            {
                StartCoroutine(Attack());
            }
        }

        private void TryOpenDoor()
        {
            if (Trust < trustThreshold)
            {
                Notice?.Invoke($"Arven kapıya uzandı ama durdu: sana henüz yeterince güvenmiyor ({Trust}/{trustThreshold}).");
                return;
            }

            if (doorActivatable != null && doorActivatable.IsActivated)
            {
                return;
            }

            doorActivatable?.Activate();
            Notice?.Invoke("Arven kenara çekildi ve kapıyı açtı. Tapınağın kalbine giden yol açık.");
        }

        private IEnumerator Attack()
        {
            Notice?.Invoke("Arven öfkeyle yumruğunu kaldırdı!");
            yield return new WaitForSeconds(attackDelay);
            playerRespawn?.Respawn();
            PlayerAttacked?.Invoke();
        }
    }
}
