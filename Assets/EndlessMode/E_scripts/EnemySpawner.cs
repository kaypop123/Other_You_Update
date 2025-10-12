using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;
    public float spawnInterval = 2f;
    private bool canSpawn = true;
    private Coroutine spawnCoroutine;

    void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (canSpawn)
        {
            // 🔹 플레이어 사망 체크
            if (HurtPlayer.Instance != null && HurtPlayer.Instance.IsDead())
            {
                Debug.Log("플레이어 사망! 스폰 중단");
                yield break; // 코루틴 종료
            }

            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, leftSpawnPoint.position, Quaternion.identity);
            Instantiate(enemyPrefab, rightSpawnPoint.position, Quaternion.identity);
        }
    }

    public void StopSpawning()
    {
        canSpawn = false;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        Debug.Log($"[{gameObject.name}] Enemy Spawning Stopped!");
    }

    public void ResumeSpawning()
    {
        if (!canSpawn)
        {
            canSpawn = true;
            spawnCoroutine = StartCoroutine(SpawnEnemies());
        }
    }
}




/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;      // 스폰할 적 프리팹
    public Transform leftSpawnPoint; // 왼쪽 스폰 위치
    public Transform rightSpawnPoint;// 오른쪽 스폰 위치
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
            Instantiate(enemyPrefab, leftSpawnPoint.transform.position, Quaternion.identity);
            Instantiate(enemyPrefab, rightSpawnPoint.transform.position, Quaternion.identity);
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

*/