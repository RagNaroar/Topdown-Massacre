using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugRayManager : MonoBehaviour
{
    [Header("Settings")]
    public bool showDebugRays = true;
    public Key toggleKey = Key.F3;
    public float lineWidth = 0.05f;
    public float defaultDuration = 0.5f;

    private struct ActiveRay { public LineRenderer lr; public float timeLeft; }
    private readonly List<ActiveRay> activeRays = new List<ActiveRay>();
    private Material rayMaterial;

    void Awake()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        rayMaterial = shader != null ? new Material(shader) : null;
    }

    // Подписка происходит только пока объект активен в сцене.
    // Выключил объект (SetActive false) или удалил со сцены — подписка сама снимается, ошибок не будет.
    void OnEnable()
    {
        ShotEvents.OnShotFired += HandleShotFired;
    }

    void OnDisable()
    {
        ShotEvents.OnShotFired -= HandleShotFired;
    }

    void HandleShotFired(Vector3 start, Vector3 end, Color color)
    {
        DrawRay(start, end, color);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            showDebugRays = !showDebugRays;
            if (!showDebugRays) ClearAllRays();
        }

        for (int i = activeRays.Count - 1; i >= 0; i--)
        {
            var ray = activeRays[i];
            ray.timeLeft -= Time.deltaTime;
            if (ray.timeLeft <= 0)
            {
                if (ray.lr != null) Destroy(ray.lr.gameObject);
                activeRays.RemoveAt(i);
            }
            else activeRays[i] = ray;
        }
    }

    public void DrawRay(Vector3 start, Vector3 end, Color color, float duration = -1f)
    {
        if (!showDebugRays || rayMaterial == null) return;

        GameObject rayObj = new GameObject("Debug_Ray_Line");
        rayObj.transform.SetParent(transform);

        LineRenderer lr = rayObj.AddComponent<LineRenderer>();
        lr.material = rayMaterial;
        lr.useWorldSpace = true;
        lr.alignment = LineAlignment.View;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.startColor = lr.endColor = color;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        activeRays.Add(new ActiveRay { lr = lr, timeLeft = duration > 0 ? duration : defaultDuration });
    }

    private void ClearAllRays()
    {
        foreach (var ray in activeRays)
            if (ray.lr != null) Destroy(ray.lr.gameObject);
        activeRays.Clear();
    }
}