using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float damage = 25f;          // Урон за один удар
    public float attackRange = 1.8f;    // Дистанция атаки (в метрах)
    public float attackCooldown = 1.2f; // Пауза между ударами

    [Header("Target Settings")]
    public string playerTag = "Player";

    private float attackTimer;
    private Transform playerTransform;
    private Health playerHealth;

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        // Если ссылка на игрока потеряна (например, при перезагрузке), ищем заново
        if (playerTransform == null || playerHealth == null)
        {
            FindPlayer();
            return;
        }

        // Отсчитываем таймер перезарядки атаки
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        // Проверяем дистанцию до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Наносим урон, если бот подошел близко и кулдаун прошел
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
    }

    // Отображение радиуса атаки в инспекторе (фиолетовая сфера при выделении)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}