using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Bir odadaki tuzakları tek tohumla başlatır. Tohum -1 ise her oyunda farklı düzen oluşur;
    /// testler belirli tohumlar vererek düzeni tekrarlanabilir kılar.
    /// </summary>
    public class SpikeTrapField : MonoBehaviour
    {
        [SerializeField] private SpikeTrap[] traps;
        [SerializeField] private Transform resetPoint;
        [SerializeField] private int seed = -1;

        public int CurrentSeed { get; private set; }

        public void Configure(SpikeTrap[] fieldTraps, Transform victimResetPoint)
        {
            traps = fieldTraps;
            resetPoint = victimResetPoint;
        }

        public void Reseed(int newSeed)
        {
            CurrentSeed = newSeed;
            var master = new System.Random(newSeed);
            foreach (SpikeTrap trap in traps)
            {
                trap.Initialize(master.Next(), resetPoint.position);
            }
        }

        private void Start()
        {
            Reseed(seed >= 0 ? seed : System.Environment.TickCount);
        }
    }
}
