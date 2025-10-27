using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject targetObj; // 현텔레포트 위치
    public GameObject toObj;     // 다음텔레포트 위치

    [Header("���� ���� ī�޶� ��� �ڽ�")]
    public BoxCollider2D nextMapBound;   // �߰���
    // [추가] 스테이지 리셋 관련 변수
    public EnemySpawner targetSpawner;   // 초기화할 스포너
    public EnemySpawner targetSpawner2;   // 초기화할 스포너2
    public SpawnTrigger targetTrigger;   // 초기화할 트리거
    public GameObject d_fireballSpawner;   // 비활성화할 오브젝트
    public GameObject s_fireballSpawner;   // 활성화할 오브젝트


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

        // 4. 파이어볼 스포너
        if (d_fireballSpawner != null)
        {
            d_fireballSpawner.SetActive(false);
        }
        if (s_fireballSpawner != null)
        {
            s_fireballSpawner.SetActive(true);
        }

        // [추가] 스테이지 초기화 로직 (텔레포트 끝난 후)
        if (targetSpawner != null)
            targetSpawner.ResetSpawner();
        if (targetSpawner2 != null)
            targetSpawner2.ResetSpawner();

        if (targetTrigger != null)
            targetTrigger.ResetTrigger();

        // 스테이지 증가 (ㅎㅈ)
        StageManager.Instance.IncreaseStageAndShow();


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