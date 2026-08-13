using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyChaser : MonoBehaviour
{
    private enum EnemyState { Idle, MovingToPulse, Searching, ChasingPlayer }

    [Header("Player Detection Settings")]
    public Transform playerTransform;
    public float sightDistance = 10f;       // Дистанция зрения
    public LayerMask obstacleLayers;        // Слой стен/препятствий (чтобы не видел сквозь стены)
    public float loseSightDuration = 3f;     // Сколько секунд помнит игрока после потери из виду

    [Header("Speed Settings")]
    public float pulseMoveSpeed = 3.5f;     // Скорость движения на звук эха
    public float searchSpeed = 2.0f;        // Скорость при обыске местности
    public float chasePlayerSpeed = 4.5f;   // Скорость прямой погони за игроком

    [Header("Search Area Settings")]
    public float totalSearchDuration = 8f;  // Время обыска перед возвратом в Idle
    public float searchRadius = 15f;        // Радиус обыска
    public float waitAtSearchPoint = 1.5f;  // Пауза между точками

    private NavMeshAgent agent;
    private EnemyState currentState = EnemyState.Idle;
    
    private Vector3 lastKnownPosition;
    private float actionTimer;
    private float searchWaitTimer;
    private float loseSightTimer;
    private Coroutine delayedReactionCoroutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // Если игрок не перетащен вручную, ищем его по тегу "Player"
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
                playerTransform = playerObj.transform;
        }
    }

    void OnEnable() => EchoPulse.OnPulseEmitted += HandlePulseHeard;
    void OnDisable() => EchoPulse.OnPulseEmitted -= HandlePulseHeard;

    void HandlePulseHeard(Vector3 pulseOrigin, float pulseRadius, float waveSpeed)
    {
        // Если бот уже своими глазами видит игрока — импульсы его не отвлекают
        if (currentState == EnemyState.ChasingPlayer) return;

        float distance = Vector3.Distance(transform.position, pulseOrigin);

        if (distance <= pulseRadius)
        {
            float delay = distance / waveSpeed;

            if (delayedReactionCoroutine != null)
                StopCoroutine(delayedReactionCoroutine);

            delayedReactionCoroutine = StartCoroutine(ReactToPulseWithDelay(pulseOrigin, delay));
        }
    }

    IEnumerator ReactToPulseWithDelay(Vector3 targetPos, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (agent.isOnNavMesh && currentState != EnemyState.ChasingPlayer)
        {
            lastKnownPosition = targetPos;
            agent.speed = pulseMoveSpeed;
            agent.SetDestination(targetPos);
            
            currentState = EnemyState.MovingToPulse;
            actionTimer = totalSearchDuration;
        }
    }

    void Update()
    {
        // 1. Постоянно проверяем, попадает ли игрок в поле зрения
        if (CanSeePlayer())
        {
            StartChasingPlayer();
        }

        // 2. Обработка текущего состояния
        switch (currentState)
        {
            case EnemyState.Idle:
                break;

            case EnemyState.MovingToPulse:
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f) { StopSearch(); break; }

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    StartSearchingArea(lastKnownPosition);
                }
                break;

            case EnemyState.Searching:
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f) { StopSearch(); break; }

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    searchWaitTimer -= Time.deltaTime;
                    if (searchWaitTimer <= 0f)
                    {
                        MoveToRandomSearchPoint();
                        searchWaitTimer = waitAtSearchPoint;
                    }
                }
                break;

            case EnemyState.ChasingPlayer:
                HandleChasingLogic();
                break;
        }
    }

    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > sightDistance) return false;

        // Пускаем луч от центра бота к игроку (чуть приподнимаем над полом)
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 targetPos = playerTransform.position + Vector3.up * 0.5f;
        Vector3 direction = (targetPos - origin).normalized;

        // Если Raycast с упором в obstacleLayers пересекает стену — бот игрока НЕ видит
        if (Physics.Raycast(origin, direction, distanceToPlayer, obstacleLayers))
        {
            return false;
        }

        return true;
    }

    void StartChasingPlayer()
    {
        currentState = EnemyState.ChasingPlayer;
        agent.speed = chasePlayerSpeed;
        loseSightTimer = loseSightDuration;
        
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    void HandleChasingLogic()
    {
        if (CanSeePlayer())
        {
            // Пока видим — бежим прямо за игроком и запоминаем его координаты
            loseSightTimer = loseSightDuration;
            lastKnownPosition = playerTransform.position;
            
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(playerTransform.position);
            }
        }
        else
        {
            // Игрок скрылся (забежал за стену/убежал далеко)
            loseSightTimer -= Time.deltaTime;

            // Если время укрытия вышло — начинаем обыскивать район вокруг ПОСЛЕДНЕЙ точки, где видели игрока
            if (loseSightTimer <= 0f)
            {
                StartSearchingArea(lastKnownPosition);
            }
        }
    }

    void StartSearchingArea(Vector3 searchOrigin)
    {
        currentState = EnemyState.Searching;
        lastKnownPosition = searchOrigin;
        actionTimer = totalSearchDuration;
        agent.speed = searchSpeed;
        searchWaitTimer = 0f;
    }

    void MoveToRandomSearchPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
        randomDirection += lastKnownPosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void StopSearch()
    {
        currentState = EnemyState.Idle;
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    // Красная сфера в редакторе Unity для наглядной настройки дистанции зрения
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightDistance);
    }
}