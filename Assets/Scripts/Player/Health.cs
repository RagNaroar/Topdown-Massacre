using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Type")]
    public bool isPlayer = false;

    [Header("Scene Settings (Only for Player)")]
    public string gameOverSceneName = "GameOverScene";

    private bool isDead = false;

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
            // Сохраняем прожитое время между сценами
            PlayerPrefs.SetFloat("LastSurvivalTime", GameTimer.SurvivalTime);
            PlayerPrefs.Save();

            // Переходим на сцену смерти
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            // Бот просто исчезает
            Destroy(gameObject);
        }
    }
}