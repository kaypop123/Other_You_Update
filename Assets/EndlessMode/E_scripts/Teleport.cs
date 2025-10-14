using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject targetObj; // 플레이어
    public GameObject toObj;     // 텔포 도착 위치

    [Header("다음 맵의 카메라 경계 박스")]
    public BoxCollider2D nextMapBound;   // 추가됨

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

        // 1. 플레이어 이동
        targetObj.transform.position = toObj.transform.position;

        // 2. 카메라 경계 갱신
        if (nextMapBound != null)
        {
            Camera.main.GetComponent<CameraLimit>().UpdateBounds(nextMapBound);
        }
    }
}



/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject targetObj; // 플레이어
    public GameObject toObj; // 텔포 장소

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