using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Type")]
    public bool isPlayer = false;
    private bool isDead = false;

    [Header("Game Over UI (Only for Player)")]
    public GameObject panel;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (isPlayer)
        {
            if (panel != null)
            {
            panel.SetActive(true);
            Time.timeScale = 0f;
            }
            GetComponent<Gun>().enabled = false;
            GetComponent<GunShotAudio>().enabled = false;
        }
        else
        {
            // Бот просто исчезает
            Destroy(gameObject);
        }
    }
}
