using Lumen.Core;
using UnityEngine;

namespace Lumen.Player
{
    /// <summary>
    /// Fareyle döndürülen, duvarlara girmeyen üçüncü şahıs kamera.
    /// Girdi kilitliyken (örn. sohbet) imleci serbest bırakır.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField] private float distance = 5f;
        [SerializeField] private float sensitivity = 3f;
        [SerializeField] private float minPitch = -15f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private float yaw;
        private float pitch = 18f;

        public void Configure(Transform followTarget, LayerMask obstacles)
        {
            target = followTarget;
            collisionMask = obstacles;
            yaw = followTarget.eulerAngles.y;
        }

        private void OnEnable()
        {
            InputLock.Changed += ApplyCursorState;
            ApplyCursorState(InputLock.IsLocked);
        }

        private void OnDisable()
        {
            InputLock.Changed -= ApplyCursorState;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (!InputLock.IsLocked)
            {
                yaw += Input.GetAxis("Mouse X") * sensitivity;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, minPitch, maxPitch);
            }

            Vector3 pivot = target.position + pivotOffset;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 back = rotation * Vector3.back;

            float actualDistance = distance;
            if (Physics.SphereCast(pivot, collisionRadius, back, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                actualDistance = Mathf.Max(0.5f, hit.distance);
            }

            transform.SetPositionAndRotation(pivot + back * actualDistance, rotation);
        }

        private static void ApplyCursorState(bool inputLocked)
        {
            Cursor.lockState = inputLocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = inputLocked;
        }
    }
}
