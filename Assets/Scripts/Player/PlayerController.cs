using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Rotation Settings")]
    [Tooltip("Поменяй на 0, 90, 180 или -90, если персонаж смотрит боком относительно курсора")]
    public float rotationOffset = 0f;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 moveInput;
    private Vector3 mouseWorldPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 1. Считываем ввод WASD для 3D-плоскости пола (X - влево/вправо, Z - вперед/назад)
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;  // Вперед по карте
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;  // Назад по карте
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;  // Влево по карте
            if (Keyboard.current.dKey.isPressed) moveX += 1f;  // Вправо по карте
        }

        moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        // 2. Находим позицию мыши на полу
        if (Mouse.current != null && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float enter))
            {
                mouseWorldPosition = ray.GetPoint(enter);
            }
        }
    }

    void FixedUpdate()
    {
        // 1. Движение по плоскости XZ
        Vector3 targetPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        // 2. Поворот за мышкой
        Vector3 lookDirection = mouseWorldPosition - transform.position;
        lookDirection.y = 0; // Игнорируем высоту для вращения

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg + rotationOffset;
            
            // Если спрайт лежит на полу (Rotation X = 90 в Transform), поворачиваем его вокруг Y:
            transform.rotation = Quaternion.Euler(90f, angle, 0f);
        }
    }
}