using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    public EnemySpawner targetSpawner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")||(collision.CompareTag("DevaPlayer") && targetSpawner != null))
        {
            Debug.Log("�÷��̾ ���� Ʈ���ſ� ����!");
            targetSpawner.StartSpawning();
            GetComponent<Collider2D>().enabled = false;
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
