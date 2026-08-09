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
        if(PauseMenu.GameIsPaused) return;
        
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
        }
        else
        {
            endPos = startPos + shootDirection.normalized * range;
        }

        // Просто сообщаем о выстреле всем, кому интересно — без прямой зависимости от DebugRayManager
        ShotEvents.RaiseShot(startPos, endPos, rayColor);
    }
}