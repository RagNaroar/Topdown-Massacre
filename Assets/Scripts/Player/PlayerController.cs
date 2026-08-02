using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Rotation Settings")]
    [Tooltip("Поменяй на 0, 90, 180 или -90, если персонаж смотрит боком относительно курсора")]
    public float rotationOffset = 0f;

    [Header("Footstep Settings")]
    public AudioClip[] footstepClips;
    public AudioSource audioSource;
    public float footstepInterval = 0.5f;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 moveInput;
    private Vector3 mouseWorldPosition;
    private float stepTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
        }

        moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        if (Mouse.current != null && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float enter))
            {
                mouseWorldPosition = ray.GetPoint(enter);
            }
        }

        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.1f;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime; // было += , исправлено на -=
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = footstepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
    if (footstepClips.Length == 0 || audioSource == null) return;

    AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
    audioSource.PlayOneShot(clip);
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        Vector3 lookDirection = mouseWorldPosition - transform.position;
        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg + rotationOffset;
            transform.rotation = Quaternion.Euler(90f, angle, 0f);
        }
    }
}