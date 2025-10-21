using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusUpgrade : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || (collision.CompareTag("DevaPlayer")))
        {
            Debug.Log("적 데미지와 체력이 증가했습니다!");
          
        }
    }
}
