using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [Header("Muzzle Flash Fix")]
    public Vector3 muzzleFlashRotationOffset = Vector3.zero;

    [Header("Gun Parameters")]
    public float damage = 25f;
    public float range = 100f;
    public LayerMask hitLayers;
    public Vector3 targetMouseWorldPos;

    [Header("References")]
    public Transform firePoint;
    public GameObject muzzleFlashPrefab;

    [Header("Debug Settings")]
    public Color rayColor = Color.green;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogError("[Gun] firePoint не назначен!");
            return;
        }

        // Вспышка — один раз, с учётом offset'а поворота
        if (muzzleFlashPrefab != null)
        {
            Quaternion flashRotation = firePoint.rotation * Quaternion.Euler(muzzleFlashRotationOffset);
            Instantiate(muzzleFlashPrefab, firePoint.position, flashRotation);
        }

        Vector3 shootDirection = firePoint.forward;
        shootDirection.y = 0;

        if (shootDirection.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("[Gun] shootDirection почти нулевой после обнуления Y — ствол смотрит вертикально.");
            shootDirection = firePoint.forward; // fallback: не обнуляем Y в этом случае
        }

        Vector3 startPos = firePoint.position;
        Vector3 endPos;

        RaycastHit hit;
        if (Physics.Raycast(startPos, shootDirection.normalized, out hit, range, hitLayers))
        {
            Debug.Log($"[Gun] Попали в: {hit.transform.name}");
            endPos = hit.point;
        }
        else
        {
            endPos = startPos + shootDirection.normalized * range;
        }

        Debug.Log($"[Gun] Shoot: start={startPos}, end={endPos}, DebugRayManager.Instance={(DebugRayManager.Instance != null)}");

        if (DebugRayManager.Instance != null)
        {
            DebugRayManager.Instance.DrawRay(startPos, endPos, rayColor);
        }
        else
        {
            Debug.LogError("[Gun] DebugRayManager.Instance == null! Луч не будет нарисован.");
        }
    }
}