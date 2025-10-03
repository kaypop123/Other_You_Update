using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;   // 스폰할 적
    public Transform leftSpawnPoint;     // 왼쪽 위치
    public Transform rightSpawnPoint;     // 오른쪽 위치
    public float spawnInterval = 2f; // 스폰 간격

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            Instantiate(enemyPrefab, leftSpawnPoint.position, Quaternion.identity);
            Instantiate(enemyPrefab, rightSpawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
