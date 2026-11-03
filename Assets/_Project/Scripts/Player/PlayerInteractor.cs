using Lumen.Core;
using UnityEngine;

namespace Lumen.Player
{
    /// <summary>
    /// Yakındaki en uygun IInteractable nesnesini bulur ve E tuşuyla etkileşime geçer.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float radius = 2.5f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float eyeHeight = 1.5f;

        private readonly Collider[] buffer = new Collider[16];

        public IInteractable Current { get; private set; }

        private void Update()
        {
            Current = FindBest();

            if (Current != null && !InputLock.IsLocked && Input.GetKeyDown(interactKey))
            {
                Current.Interact(gameObject);
            }
        }

        private IInteractable FindBest()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, buffer, GameLayers.InteractableMask, QueryTriggerInteraction.Collide);
            IInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                IInteractable candidate = buffer[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract(gameObject) || !InSight(buffer[i]))
                {
                    continue;
                }

                float d = (buffer[i].transform.position - transform.position).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = candidate;
                }
            }

            return best;
        }

        // Göz hizasından nesnenin en yakın noktasına çizgi çekilir; arada duvar (Default katmanı) varsa etkileşim yok.
        // Böylece duvarın arkasındaki kristal ya da yazıt, küre içinde kalsa bile kullanılamaz.
        private bool InSight(Collider target)
        {
            Vector3 eye = transform.position + Vector3.up * eyeHeight;
            Vector3 point = target.ClosestPoint(eye);
            return !Physics.Linecast(eye, point, 1 << 0, QueryTriggerInteraction.Ignore);
        }
    }
}
