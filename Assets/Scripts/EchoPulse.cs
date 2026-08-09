using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class EchoPulse : MonoBehaviour
{
    [Header("Wave Settings")]
    public float maxRadius = 15f;
    public float waveSpeed = 10f; // юниты в секунду
    public LayerMask revealLayers;

    [Header("Cooldown Settings")]
    public float cooldownTime = 3f;
    private float cooldownTimer = 0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pulseSound;

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

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

        StartCoroutine(ExpandWave());
    }

    IEnumerator ExpandWave()
    {
        float currentRadius = 0f;
        HashSet<Collider> alreadyRevealed = new HashSet<Collider>();

        while (currentRadius < maxRadius)
        {
            currentRadius += waveSpeed * Time.deltaTime;

            Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius, revealLayers);

            foreach (var hit in hits)
            {
                if (alreadyRevealed.Contains(hit)) continue;

                EchoRevealable revealableWall = hit.GetComponent<EchoRevealable>();
                if (revealableWall != null)
                {
                    revealableWall.Reveal();
                    alreadyRevealed.Add(hit);
                    continue;
                }

                EchoRevealableSprite revealableEnemy = hit.GetComponent<EchoRevealableSprite>();
                if (revealableEnemy != null)
                {
                    revealableEnemy.Reveal();
                    alreadyRevealed.Add(hit);
                }
            }

            yield return null;
        }
    }
}