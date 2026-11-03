using Lumen.Core;
using UnityEngine;

namespace Lumen.Player
{
    /// <summary>
    /// Göz hizasında birinci şahıs kamera: fare yatayda (yaw) ve dikeyde (pitch) bakışı çevirir.
    /// Oyuncu WASD ile kameranın baktığı yöne yürür (PlayerController kameranın yönünü kullanır).
    /// Girdi kilitliyken (sohbet, menü) bakış durur ve imleç serbest kalır.
    /// </summary>
    public class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float eyeHeight = 1.6f;
        [SerializeField] private float sensitivity = 3f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        private float yaw;
        private float pitch;

        public void Configure(Transform player)
        {
            target = player;
            yaw = player.eulerAngles.y;
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

            transform.SetPositionAndRotation(target.position + Vector3.up * eyeHeight, Quaternion.Euler(pitch, yaw, 0f));
        }

        private static void ApplyCursorState(bool inputLocked)
        {
            Cursor.lockState = inputLocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = inputLocked;
        }
    }
}
