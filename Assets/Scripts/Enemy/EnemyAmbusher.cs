using UnityEngine;
using UnityEngine.AI;

public class EnemyAmbusher : MonoBehaviour
{
    [Header("Ambush Settings")]
    public float detectionRadius = 4f;
    public float pounceSpeed = 12f;
    public float pounceDuration = 0.3f;
    public LayerMask playerLayer;

    private NavMeshAgent agent;
    private bool isPouncing = false;
    private float pounceTimer;
    private Vector3 pounceDirection;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isPouncing)
        {
            HandlePounce();
            return;
        }

        CheckForPlayer();
    }

    void CheckForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        if (hits.Length > 0)
        {
            StartPounce(hits[0].transform.position);
        }
    }

    void StartPounce(Vector3 targetPosition)
    {
        isPouncing = true;
        pounceTimer = pounceDuration;

        pounceDirection = (targetPosition - transform.position).normalized;
        pounceDirection.y = 0; // Исключаем смещение по вертикали для 2.5D/3D

        // Отключаем навигацию агента на время ручного рывка
        agent.enabled = false; 
    }

    void HandlePounce()
    {
        pounceTimer -= Time.deltaTime;

        // Двигаем через Transform или Rigidbody пока выключен NavMeshAgent
        transform.position += pounceDirection * pounceSpeed * Time.deltaTime;

        if (pounceTimer <= 0f)
        {
            isPouncing = false;
            agent.enabled = true; // Включаем обратно
        }
    }
}