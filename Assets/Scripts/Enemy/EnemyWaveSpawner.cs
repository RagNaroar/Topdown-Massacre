using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject[] enemyPrefabs;

    [Header("Spawn Radius")]
    public float minSpawnRadius = 8f;
    public float maxSpawnRadius = 15f;

    [Header("Wave Settings")]
    public int enemiesPerWave = 5;
    public float timeBetweenSpawns = 0.5f;
    public float delayBeforeNextWave = 3f; // пауза ПОСЛЕ того, как волна полностью умерла

    [Header("NavMesh Settings")]
    public float navMeshSampleDistance = 2f;

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("[EnemyWaveSpawner] Player не назначен!");
            return;
        }

        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(SpawnWave());

            // Ждём, пока ВСЕ враги этой волны не умрут
            yield return StartCoroutine(WaitUntilWaveCleared());

            yield return new WaitForSeconds(delayBeforeNextWave);
        }
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnSingleEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    IEnumerator WaitUntilWaveCleared()
    {
        while (true)
        {
            // Убираем из списка всех, кто уже уничтожен (null после Destroy)
            activeEnemies.RemoveAll(enemy => enemy == null);

            if (activeEnemies.Count == 0)
            {
                yield break; // волна полностью зачищена, выходим из ожидания
            }

            yield return new WaitForSeconds(0.5f); // проверяем каждые пол секунды, не грузим CPU каждый кадр
        }
    }

    void SpawnSingleEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        Vector3 spawnPos;
        if (!TryGetSpawnPosition(out spawnPos))
        {
            Debug.LogWarning("[EnemyWaveSpawner] Не удалось найти валидную точку спавна.");
            return;
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    bool TryGetSpawnPosition(out Vector3 result)
    {
        const int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            Vector3 candidatePos = player.position + offset;

            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }
}