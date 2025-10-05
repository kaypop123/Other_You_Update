using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;      // 스폰할 적 프리팹
    public float spawnInterval = 2f;    // 스폰 간격
    private bool canSpawn = true;       // 스폰 가능 여부

    private Coroutine spawnCoroutine;

    void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (canSpawn)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }
    }

    // 🔹 스폰 중단 함수
    public void StopSpawning()
    {
        canSpawn = false;

        // 코루틴 중단
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        Debug.Log($"[{gameObject.name}] Enemy Spawning Stopped!");
    }

    // 🔹 스폰 재개 함수 (옵션)
    public void ResumeSpawning()
    {
        if (!canSpawn)
        {
            canSpawn = true;
            spawnCoroutine = StartCoroutine(SpawnEnemies());
        }
    }
}
