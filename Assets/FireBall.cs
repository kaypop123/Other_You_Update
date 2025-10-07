using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float speed = 3f;
    public Collider2D frontCollider; // 벽 감지용
    private EdgeCollider2D terrainCollider;
    private Vector2 vx;
    private SpriteRenderer sr;
    private BoxCollider2D boxCol; // 기존 BoxCollider2D 활용

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        boxCol = GetComponent<BoxCollider2D>();

        if (boxCol != null)
            boxCol.enabled = false; // 페이드 전 비활성

        vx = Vector2.right * speed;

        // Grid 안의 Tilemap 안에서 EdgeCollider2D 탐색
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj != null)
        {
            terrainCollider = gridObj.GetComponentInChildren<EdgeCollider2D>();
            if (terrainCollider == null)
                Debug.LogWarning("Grid 하위 Tilemap에서 EdgeCollider2D를 찾지 못했습니다.");
        }
        else
        {
            Debug.LogWarning("'Grid' 오브젝트를 찾지 못했습니다.");
        }

        StartCoroutine(FadeInFireBall(2f)); // 페이드 시작
    }

    IEnumerator FadeInFireBall(float duration)
    {
        if (sr == null) yield break;

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / duration);
            sr.color = c;
            yield return null;
        }

        c.a = 1f;
        sr.color = c;

        if (boxCol != null)
            boxCol.enabled = true; // 완전히 불투명해지면 활성
    }

    void Update()
    {
        if (terrainCollider != null && frontCollider.IsTouching(terrainCollider))
        {
            vx = -vx;
            transform.localScale = new Vector2(-transform.localScale.x, 1);
        }
    }

    private void FixedUpdate()
    {
        transform.Translate(vx * Time.fixedDeltaTime);
    }
}



//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FireBall : MonoBehaviour
//{
//    public float speed = 3f;
//    public Collider2D frontCollider;
//    private EdgeCollider2D terrainCollider;
//    private Vector2 vx;


//    void Start()
//    {
//        vx = Vector2.right * speed;

//        // Grid 안의 Tilemap 안에서 EdgeCollider2D 탐색
//        GameObject gridObj = GameObject.Find("Grid");
//        if (gridObj != null)
//        {
//            // 하위 모든 자식 중에서 EdgeCollider2D 탐색
//            terrainCollider = gridObj.GetComponentInChildren<EdgeCollider2D>();
//            if (terrainCollider == null)
//            {
//                Debug.LogWarning(" Grid 하위 Tilemap에서 EdgeCollider2D를 찾지 못했습니다.");
//            }
//        }
//        else
//        {
//            Debug.LogWarning("'Grid' 오브젝트를 찾지 못했습니다.");
//        }
//    }

//    void Update()
//    {
//        if (terrainCollider != null && frontCollider.IsTouching(terrainCollider))
//        {
//            vx = -vx;
//            transform.localScale = new Vector2(-transform.localScale.x, 1);
//        }
//    }

//    private void FixedUpdate()
//    {
//        transform.Translate(vx * Time.fixedDeltaTime);
//    }
//}



