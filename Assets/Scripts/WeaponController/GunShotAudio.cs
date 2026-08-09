using UnityEngine;
using UnityEngine.InputSystem;

public class GunShotAudio : MonoBehaviour
{
    public AudioClip shotClip;
    public AudioSource audioSource;

    void Update()
    {
        if (PauseMenu.GameIsPaused) return;
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlayShotSound();
        }
    }

    void PlayShotSound()
    {
        if (shotClip == null || audioSource == null) return;
        audioSource.PlayOneShot(shotClip);
    }
}