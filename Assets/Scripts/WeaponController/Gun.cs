using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class Gun : MonoBehaviour
{
    [Header("Muzzle Flash Fix")]
    public Vector3 muzzleFlashRotationOffset = Vector3.zero;

    [Header("Gun Parameters")]
    public float damage = 25f;
    public float range = 100f;
    public float knockback = 0.4f; // Сила отброса назад
    public LayerMask hitLayers;
    public Vector3 targetMouseWorldPos;

    [Header("References")]
    public Transform firePoint;
    public GameObject muzzleFlashPrefab;

    [Header("Debug Settings")]
    public Color rayColor = Color.green;

    void Update()
    {
        if (PauseMenu.GameIsPaused) return;
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Camera.main.GetComponent<CameraController>().Shake();
        if (firePoint == null)
        {
            Debug.LogError("[Gun] firePoint не назначен!");
            return;
        }

        if (muzzleFlashPrefab != null)
        {
            Quaternion flashRotation = firePoint.rotation * Quaternion.Euler(muzzleFlashRotationOffset);
            Instantiate(muzzleFlashPrefab, firePoint.position, flashRotation);
        }

        Vector3 shootDirection = firePoint.forward;
        shootDirection.y = 0;

        if (shootDirection.sqrMagnitude < 0.0001f)
        {
            shootDirection = firePoint.forward;
        }

        Vector3 startPos = firePoint.position;
        Vector3 endPos;

        if (Physics.Raycast(startPos, shootDirection.normalized, out RaycastHit hit, range, hitLayers))
        {
            endPos = hit.point;
            Health targetHealth = hit.collider.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }

            ApplyKnockback(hit.collider.gameObject, shootDirection.normalized);
        }
        else
        {
            endPos = startPos + shootDirection.normalized * range;
        }

        ShotEvents.RaiseShot(startPos, endPos, rayColor);
    }

    void ApplyKnockback(GameObject target, Vector3 direction)
    {
    // 1. Если бот передвигается через NavMeshAgent (наш случай)
    NavMeshAgent agent = target.GetComponentInParent<NavMeshAgent>();
    if (agent != null && agent.isOnNavMesh)
    {
        // Мгновенный резкий сдвиг назад по сетке на 40 см без долгого скольжения
        agent.Move(direction * knockback);
        return;
    }

    // 2. Если на боте висит физический Rigidbody
    Rigidbody rb = target.GetComponentInParent<Rigidbody>();
    if (rb != null && !rb.isKinematic)
    {
        // Гасим текущую скорость, чтобы он не летел по инерции, и даем короткий толчок
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * 2f, ForceMode.Impulse);
    }
    }

}