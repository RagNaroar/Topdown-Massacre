using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class EchoPulse : MonoBehaviour
{
    public static event Action<Vector3, float, float> OnPulseEmitted; // добавили waveSpeed

    [Header("Wave Settings")]
    public float maxRadius = 15f;
    public float waveSpeed = 10f;
    public LayerMask revealLayers;

    [Header("Cooldown Settings")]
    public float cooldownTime = 3f;
    private float cooldownTimer = 0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pulseSound;

    private Collider[] hitBuffer = new Collider[50]; // Буфер для избежания GC

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (cooldownTimer <= 0f)
            {
                EmitPulse();
                cooldownTimer = cooldownTime;
            }
        }
    }

    void EmitPulse()
    {
        if (audioSource != null && pulseSound != null)
            audioSource.PlayOneShot(pulseSound);

        // Передаем позицию, радиус и скорость волны
        OnPulseEmitted?.Invoke(transform.position, maxRadius, waveSpeed);

        StartCoroutine(ExpandWave());
    }

    IEnumerator ExpandWave()
    {
        float currentRadius = 0f;
        HashSet<Collider> alreadyRevealed = new HashSet<Collider>();

        while (currentRadius < maxRadius)
        {
            currentRadius += waveSpeed * Time.deltaTime;
            int numHits = Physics.OverlapSphereNonAlloc(transform.position, currentRadius, hitBuffer, revealLayers);

            for (int i = 0; i < numHits; i++)
            {
                Collider hit = hitBuffer[i];
                if (alreadyRevealed.Contains(hit)) continue;

                // Универсальная проверка через GetComponent
                if (hit.TryGetComponent<EchoRevealable>(out var revealableWall))
                {
                    revealableWall.Reveal();
                    alreadyRevealed.Add(hit);
                }
                else if (hit.TryGetComponent<EchoRevealableSprite>(out var revealableEnemy))
                {
                    revealableEnemy.Reveal();
                    alreadyRevealed.Add(hit);
                }
            }

            yield return null;
        }
    }
}