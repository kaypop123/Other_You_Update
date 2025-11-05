using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    public EnemySpawner targetSpawner;
    public EnemySpawner targetSpawner2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetSpawner != null)
        {
            if ((collision.CompareTag("Player") || (collision.CompareTag("DevaPlayer")) && targetSpawner != null))
            {
                Debug.Log("포탈 1에서 소환!");
                targetSpawner.StartSpawning();
                GetComponent<Collider2D>().enabled = false;
            }
        }
        
        if (targetSpawner2 != null)
        {
            if ((collision.CompareTag("Player") || (collision.CompareTag("DevaPlayer")) && targetSpawner2 != null))
            {
                Debug.Log("포탈 2에서 소환!");
                targetSpawner2.StartSpawning();
                GetComponent<Collider2D>().enabled = false;
            }
        }
        
    }

    // [추가] 트리거 다시 작동 가능하게 리셋
    public void ResetTrigger()
    {
        // 트리거 Collider 비활성/활성으로 리셋

        GetComponent<Collider2D>().enabled = true;
        Debug.Log($"[{gameObject.name}] 트리거 리셋 완료");
    }

}
