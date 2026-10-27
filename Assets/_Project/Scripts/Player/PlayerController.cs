using Lumen.Core;
using UnityEngine;

namespace Lumen.Player
{
    /// <summary>
    /// Kameraya göre WASD hareketi. Zıplama yoktur; uçurumu oyuncu kendi başına geçemez.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float gravity = -20f;

        private CharacterController characterController;
        private float verticalVelocity;

        public void Configure(Transform cameraTarget)
        {
            cameraTransform = cameraTarget;
        }

        /// <summary>CharacterController'ı kapatıp açarak oyuncuyu güvenle ışınlar.</summary>
        public void Teleport(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
            verticalVelocity = 0f;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 move = ReadPlanarInput() * moveSpeed;

            if (move.sqrMagnitude > 0.01f)
            {
                Quaternion look = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            move.y = verticalVelocity;
            characterController.Move(move * Time.deltaTime);
        }

        private Vector3 ReadPlanarInput()
        {
            if (InputLock.IsLocked)
            {
                return Vector3.zero;
            }

            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            Vector3 forward = cameraTransform != null
                ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized
                : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            return Vector3.ClampMagnitude(forward * z + right * x, 1f);
        }
    }
}
