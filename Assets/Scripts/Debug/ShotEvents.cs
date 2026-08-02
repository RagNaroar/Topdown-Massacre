using System;
using UnityEngine;

public static class ShotEvents
{
    public static event Action<Vector3, Vector3, Color> OnShotFired;

    public static void RaiseShot(Vector3 start, Vector3 end, Color color)
    {
        OnShotFired?.Invoke(start, end, color);
    }
}