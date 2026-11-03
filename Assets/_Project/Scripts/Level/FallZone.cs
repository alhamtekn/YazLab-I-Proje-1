using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Karanlık odadaki çukurların dibi: içine düşen oyuncu ya da Lumi odanın girişine döner.
    /// KillZone'dan farkı, son kontrol noktasına değil verilen noktaya döndürmesidir.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FallZone : MonoBehaviour
    {
        [SerializeField] private Transform resetPoint;

        public void Configure(Transform reset)
        {
            resetPoint = reset;
        }

        private void OnTriggerEnter(Collider other)
        {
            other.GetComponentInParent<IHazardVictim>()?.OnHazardHit(resetPoint.position);
        }
    }
}
