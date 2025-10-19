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
        if (collision.CompareTag("Player"))
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

        // 1. �÷��̾� �̵�
        targetObj.transform.position = toObj.transform.position;

        // 2. ī�޶� ��� ����
        if (nextMapBound != null)
        {
            Camera.main.GetComponent<CameraLimit>().UpdateBounds(nextMapBound);
        }

        // 3. ��Ż ��Ȱ��ȭ
        gameObject.SetActive(false);

        // [추가] 스테이지 초기화 로직 (텔레포트 끝난 후)
        if (targetSpawner != null)
            targetSpawner.ResetSpawner();

        if (targetTrigger != null)
            targetTrigger.ResetTrigger();

        Debug.Log("포탈 이동 후 스테이지 초기화 완료!");

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