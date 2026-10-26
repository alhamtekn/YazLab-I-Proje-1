using UnityEngine;

// Birinci şahıs oyuncu: WASD ile yürür, fare sağa-sola oyuncuyu, yukarı-aşağı kamerayı döndürür.
// Kamera, Player'ın çocuğudur ve göz hizasında durur; oyuncu dönünce kamera da onunla döner.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4.5f;     
    public float mouseSensitivity = 3f; // fare hassasiyeti
    public float gravity = -20f;        // yerçekimi ivmesi
    public Transform playerCamera;      // Inspector'dan Main Camera sürüklenir
    public float minPitch = -80f;       // en fazla bu kadar yukarı bakılır
    public float maxPitch = 80f;        // en fazla bu kadar aşağı bakılır

    private CharacterController controller;
    private float verticalSpeed;        // düşerken aşağı doğru hız
    private float pitch;                // kameranın şu anki yukarı-aşağı açısı (0 = düz ileri)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // fareyi oyun penceresine kilitle ve gizle
    }

    void Update()
    {
        // 1) Fareyle sağa-sola dön
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(0f, mouseX * mouseSensitivity, 0f);

        // 1b) Fareyle yukarı-aşağı bak: yalnızca kamera eğilir, oyuncunun gövdesi dik kalır
        float mouseY = Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch - mouseY * mouseSensitivity, minPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // 2) Klavye girdisini oku: -1 ile 1 arasında değerler
        float x = Input.GetAxis("Horizontal"); // A / D
        float z = Input.GetAxis("Vertical");   // W / S

        // 3) Oyuncunun baktığı yöne göre hareket yönünü hesapla
        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f) * moveSpeed; // çapraz giderken hızlanmasın

        // 4) Yerçekimi: yerdeyken hafifçe aşağı bastır, havadayken hızlanarak düş
        if (controller.isGrounded && verticalSpeed < 0f)
        {
            verticalSpeed = -2f;
        }
        verticalSpeed += gravity * Time.deltaTime;
        move.y = verticalSpeed;

        // 5) Hareketi uygula (Time.deltaTime: kare hızından bağımsız olsun diye)
        controller.Move(move * Time.deltaTime);
    }
}
