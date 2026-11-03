using System;
using System.Collections;
using Lumen.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.Level
{
    /// <summary>
    /// Labirent kapısı: Lumi'nin şalteriyle zemine iner. openSeconds sıfırdan büyükse o kadar süre sonra
    /// yeniden kapanır (süreli kapı); oyuncu hızlı olmalıdır. Kapalıyken collider ve NavMeshObstacle geçişi keser.
    /// </summary>
    public class MazeGate : MonoBehaviour, IActivatable
    {
        [SerializeField] private Transform door;
        [SerializeField] private float openSeconds;
        [SerializeField] private float moveDuration = 0.6f;
        [SerializeField] private float sinkDepth = 4.1f;

        private Vector3 closedPosition;
        private Coroutine routine;

        public event Action Activated;

        public event Action Closed;

        public bool IsActivated { get; private set; }

        public float OpenSeconds => openSeconds;

        public void Configure(Transform doorLeaf, float secondsOpen)
        {
            door = doorLeaf;
            openSeconds = secondsOpen;
        }

        public void Activate()
        {
            if (IsActivated)
            {
                return;
            }

            IsActivated = true;
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(OpenThenMaybeClose());
        }

        private void Awake()
        {
            closedPosition = door.localPosition;
        }

        private IEnumerator OpenThenMaybeClose()
        {
            SetBlocking(false);
            yield return Move(closedPosition + Vector3.down * sinkDepth);
            Activated?.Invoke();

            if (openSeconds <= 0f)
            {
                yield break;
            }

            yield return new WaitForSeconds(openSeconds);
            yield return Move(closedPosition);
            SetBlocking(true);
            IsActivated = false;
            Closed?.Invoke();
        }

        private IEnumerator Move(Vector3 target)
        {
            Vector3 from = door.localPosition;
            for (float t = 0f; t < 1f; t += Time.deltaTime / moveDuration)
            {
                door.localPosition = Vector3.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            door.localPosition = target;
        }

        private void SetBlocking(bool blocking)
        {
            Collider doorCollider = door.GetComponent<Collider>();
            if (doorCollider != null)
            {
                doorCollider.enabled = blocking;
            }

            NavMeshObstacle obstacle = door.GetComponent<NavMeshObstacle>();
            if (obstacle != null)
            {
                obstacle.enabled = blocking;
            }
        }
    }
}
