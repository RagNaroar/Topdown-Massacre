using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class EnemyFootsteps : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Timing & Speed Settings")]
    [Tooltip("Интервал между шагами при стандартной ходьбе (например, при скорости 3.5)")]
    [SerializeField] private float baseStepInterval = 0.45f;
    
    [Tooltip("Скорость, относительно которой расчитывается базовая частота шагов")]
    [SerializeField] private float referenceSpeed = 3.5f;
    
    [Tooltip("Порог скорости, ниже которого шаги не воспроизводятся")]
    [SerializeField] private float minVelocityThreshold = 0.2f;

    private NavMeshAgent agent;
    private float stepTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        // Считываем реальную скорость перемещения агента
        float currentSpeed = agent.velocity.magnitude;

        // Воспроизводим звук только когда агент на сетке и реально двигается
        if (agent.isOnNavMesh && currentSpeed > minVelocityThreshold)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayRandomFootstep();

                // Автоматический динамический интервал:
                // Чем быстрее бежит бот, тем короче пауза между шагами
                float speedFactor = Mathf.Max(currentSpeed / referenceSpeed, 0.5f);
                stepTimer = baseStepInterval / speedFactor;
            }
        }
        else
        {
            // Если бот стоял на месте, сбрасываем таймер, 
            // чтобы первый шаг прозвучал мгновенно при старте движения
            stepTimer = 0f;
        }
    }

    private void PlayRandomFootstep()
    {
        if (audioSource == null || footstepClips == null || footstepClips.Length == 0) return;

        // Выбор случайного сэмпла
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        
        // Случайный питч (высота тона) для избавления от монотонности
        audioSource.pitch = Random.Range(0.88f, 1.12f);
        audioSource.PlayOneShot(clip);
    }
}