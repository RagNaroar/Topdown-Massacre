using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugRayManager : MonoBehaviour
{
    public static DebugRayManager Instance { get; private set; }

    [Header("Toggle Debug Rays")]
    [Tooltip("Показывать ли отладочные лучи в игре")]
    public bool showDebugRays = true;

    [Header("Settings")]
    [Tooltip("Клавиша для вкл/выкл отладки на лету")]
    public Key toggleKey = Key.F3;

    [Tooltip("Толщина луча")]
    public float lineWidth = 0.05f;

    [Tooltip("Время жизни луча по умолчанию (в секундах)")]
    public float defaultDuration = 0.5f;

    [Header("Debug")]
    [Tooltip("Выводить в консоль лог каждого вызова DrawRay")]
    public bool logCalls = true;

    private class ActiveRay
    {
        public LineRenderer lineRenderer;
        public float timeToLive;
    }

    private List<ActiveRay> activeRays = new List<ActiveRay>();
    private Material rayMaterial;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rayMaterial = CreateRayMaterial();
    }

    // Пробуем найти шейдер, который точно отрендерится в текущем Render Pipeline
    private Material CreateRayMaterial()
    {
        // Порядок важен: сначала пробуем шейдеры под URP/HDRP, потом Built-in
        string[] shaderNames = new string[]
        {
            "Universal Render Pipeline/Unlit", // URP
            "HDRP/Unlit",                       // HDRP
            "Sprites/Default",                  // Built-in (работает и в URP как фолбэк)
            "Unlit/Color"
        };

        foreach (var name in shaderNames)
        {
            Shader shader = Shader.Find(name);
            if (shader != null)
            {
                if (logCalls) Debug.Log($"[DebugRayManager] Используется шейдер: {name}");
                return new Material(shader);
            }
        }

        Debug.LogError("[DebugRayManager] Не найден ни один подходящий шейдер! Луч не будет виден.");
        return null;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            showDebugRays = !showDebugRays;
            Debug.Log($"[DebugRayManager] showDebugRays = {showDebugRays}");

            if (!showDebugRays)
            {
                ClearAllRays();
            }
        }

        for (int i = activeRays.Count - 1; i >= 0; i--)
        {
            ActiveRay ray = activeRays[i];
            ray.timeToLive -= Time.deltaTime;

            if (ray.timeToLive <= 0)
            {
                if (ray.lineRenderer != null)
                {
                    Destroy(ray.lineRenderer.gameObject);
                }
                activeRays.RemoveAt(i);
            }
        }
    }

    public void DrawRay(Vector3 start, Vector3 end, Color color, float duration = -1f)
    {
        if (logCalls) Debug.Log($"[DebugRayManager] DrawRay вызван: {start} -> {end}, showDebugRays={showDebugRays}");

        if (!showDebugRays) return;

        if (rayMaterial == null)
        {
            Debug.LogError("[DebugRayManager] rayMaterial == null, луч не будет отрисован.");
            return;
        }

        float lifetime = duration > 0 ? duration : defaultDuration;

        GameObject rayObj = new GameObject("Debug_Ray_Line");
        rayObj.layer = gameObject.layer; // на всякий случай синхронизируем слой с менеджером
        rayObj.transform.SetParent(transform, worldPositionStays: true);

        LineRenderer lr = rayObj.AddComponent<LineRenderer>();
        lr.material = rayMaterial;
        lr.useWorldSpace = true;                 // критично: не зависим от родительского transform
        lr.alignment = LineAlignment.View;        // луч всегда развёрнут к камере
        lr.textureMode = LineTextureMode.Stretch;
        lr.numCapVertices = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = color;
        lr.endColor = color;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Гарантируем, что рисуется поверх большинства объектов и не режется сортировкой
        lr.sortingOrder = 100;

        activeRays.Add(new ActiveRay { lineRenderer = lr, timeToLive = lifetime });
    }

    private void ClearAllRays()
    {
        foreach (var ray in activeRays)
        {
            if (ray.lineRenderer != null)
            {
                Destroy(ray.lineRenderer.gameObject);
            }
        }
        activeRays.Clear();
    }
}