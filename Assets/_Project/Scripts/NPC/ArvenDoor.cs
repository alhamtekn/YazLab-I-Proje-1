using System;
using System.Collections;
using Lumen.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.NPC
{
    // Arven'in arkasındaki taş kapı. GameStateManager açtırır: kapı zemine iner, ışık yanar, yol açılır.
    public sealed class ArvenDoor : MonoBehaviour, IActivatable
    {
        [SerializeField] private Transform door;
        [SerializeField] private Light doorLight;
        [SerializeField] private float sinkDepth = 3.4f;
        [SerializeField] private float openDuration = 2.5f;

        public event Action Activated;

        public bool IsActivated { get; private set; }

        public void Configure(Transform doorLeaf, Light light)
        {
            door = doorLeaf;
            doorLight = light;
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

        private void Awake() => doorLight.enabled = false;

        private IEnumerator Open()
        {
            doorLight.enabled = true;
            door.GetComponent<Collider>().enabled = false;     // oyuncu geçebilsin
            door.GetComponent<NavMeshObstacle>().enabled = false; // Lumi geçebilsin

            Vector3 from = door.localPosition;
            Vector3 to = from + Vector3.down * sinkDepth;
            for (float t = 0f; t < 1f; t += Time.deltaTime / openDuration)
            {
                door.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            door.gameObject.SetActive(false);
            Activated?.Invoke();
        }
    }
}
