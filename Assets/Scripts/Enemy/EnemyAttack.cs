using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float damage = 25f;          // Урон за один удар
    public float attackRange = 1.8f;    // Дистанция атаки (в метрах)
    public float attackCooldown = 1.2f; // Пауза между ударами

    [Header("Target Settings")]
    public string playerTag = "Player";

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] attackSounds; // можно несколько вариантов звука удара

    private float attackTimer;
    private Transform playerTransform;
    private Health playerHealth;

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (playerTransform == null || playerHealth == null)
        {
            FindPlayer();
            return;
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerHealth = playerObj.GetComponent<Health>();
        }
    }

    private void PerformAttack()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"[EnemyAttack] Бот ударил игрока на {damage} HP! У игрока осталось: {playerHealth.currentHealth}");
        }

        PlayAttackSound();
    }

    private void PlayAttackSound()
    {
        if (audioSource == null || attackSounds.Length == 0) return;

        AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}