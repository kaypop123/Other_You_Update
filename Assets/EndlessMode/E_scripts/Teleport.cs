using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject targetObj; // �÷��̾�
    public GameObject toObj;     // ���� ���� ��ġ

    [Header("���� ���� ī�޶� ��� �ڽ�")]
    public BoxCollider2D nextMapBound;   // �߰���
    // [추가] 스테이지 리셋 관련 변수
    public EnemySpawner targetSpawner;   // 초기화할 스포너
    public SpawnTrigger targetTrigger;   // 초기화할 트리거


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("DevaPlayer"))
        {
            targetObj = collision.gameObject;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")||collision.CompareTag("DevaPlayer"))
        {
            StartCoroutine(TeleportRoutine());
        }
    }

    IEnumerator TeleportRoutine()
    {
        yield return null;

        // 1. 플레이어 이동
        targetObj.transform.position = toObj.transform.position;

        // 2. 카메라 경계 갱신
        if (nextMapBound != null)
        {
            Camera.main.GetComponent<CameraLimit>().UpdateBounds(nextMapBound);
        }

        // 3. 포탈 비활성화
        gameObject.SetActive(false);

        // [추가] 스테이지 초기화 로직 (텔레포트 끝난 후)
        if (targetSpawner != null)
            targetSpawner.ResetSpawner();

        if (targetTrigger != null)
            targetTrigger.ResetTrigger();

        // =======================
        //  스테이지 리셋 후 상태 로깅
        // =======================
        if (targetSpawner != null)
        {
            Debug.Log(
                 $"[Teleport] Reset 후 상태 → " +
                 $"Spawned: {targetSpawner.GetType().GetField("currentSpawned", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(targetSpawner)}, " +
                 $"Dead: {targetSpawner.GetType().GetField("deadCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(targetSpawner)}"
                 );

            Debug.Log("포탈 이동 후 스테이지 초기화 완료!");
        }
    }
}


/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject targetObj; // �÷��̾�
    public GameObject toObj; // ���� ���

    Animator animator;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            targetObj = collision.gameObject;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(TeleportRoutine());
        }
    }

    IEnumerator TeleportRoutine()
    {
        yield return null;
        targetObj.transform.position = toObj.transform.position;
    }
}
*/