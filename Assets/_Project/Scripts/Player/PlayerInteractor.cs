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
                if (candidate == null || !candidate.CanInteract(gameObject))
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
    }
}
