using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    public EnemySpawner targetSpawner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && targetSpawner != null)
        {
            Debug.Log("플레이어가 스폰 트리거에 진입!");
            targetSpawner.StartSpawning();
            Destroy(gameObject); // 한 번만 작동하게
        }
    }
}
