using System;
using System.Collections;
using Lumen.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.Level
{
    /// <summary>
    /// Ara bölge ile bekçi bölgesi arasındaki kapı. Kristal sayacı tamamlanınca etkinleşir:
    /// yuvalar sırayla yanar, kapı kanadı zemine iner ve NavMeshObstacle kapanır.
    /// </summary>
    public class CrystalGate : MonoBehaviour, IActivatable
    {
        [SerializeField] private Transform door;
        [SerializeField] private Renderer[] sockets = Array.Empty<Renderer>();
        [SerializeField] private Material litSocketMaterial;
        [SerializeField] private Light gateLight;
        [SerializeField] private float sinkDepth = 3.4f;
        [SerializeField] private float openDuration = 2.5f;

        public event Action Activated;

        public bool IsActivated { get; private set; }

        public void Configure(Transform doorLeaf, Renderer[] socketRenderers, Material litSocket, Light light)
        {
            door = doorLeaf;
            sockets = socketRenderers;
            litSocketMaterial = litSocket;
            gateLight = light;
        }

        public void Activate()
        {
            if (IsActivated)
            {
                return;
            }

            IsActivated = true;
            StartCoroutine(Open());
        }

        private void Awake()
        {
            if (gateLight != null)
            {
                gateLight.enabled = false;
            }
        }

        private IEnumerator Open()
        {
            foreach (Renderer socket in sockets)
            {
                socket.sharedMaterial = litSocketMaterial;
                yield return new WaitForSeconds(0.3f);
            }

            if (gateLight != null)
            {
                gateLight.enabled = true;
            }

            Collider doorCollider = door.GetComponent<Collider>();
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }

            NavMeshObstacle obstacle = door.GetComponent<NavMeshObstacle>();
            if (obstacle != null)
            {
                obstacle.enabled = false;
            }

            Vector3 from = door.localPosition;
            Vector3 to = from + Vector3.down * sinkDepth;
            for (float t = 0f; t < 1f; t += Time.deltaTime / openDuration)
            {
                door.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            door.localPosition = to;
            door.gameObject.SetActive(false);
            Activated?.Invoke();
        }
    }
}
