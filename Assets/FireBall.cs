using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float speed = 3f;
    public Collider2D frontCollider;
    private EdgeCollider2D terrainCollider;
    private Vector2 vx;

    void Start()
    {
        vx = Vector2.right * speed;

        // Grid 안의 Tilemap 안에서 EdgeCollider2D 탐색
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj != null)
        {
            // 하위 모든 자식 중에서 EdgeCollider2D 탐색
            terrainCollider = gridObj.GetComponentInChildren<EdgeCollider2D>();
            if (terrainCollider == null)
            {
                Debug.LogWarning(" Grid 하위 Tilemap에서 EdgeCollider2D를 찾지 못했습니다.");
            }
        }
        else
        {
            Debug.LogWarning("'Grid' 오브젝트를 찾지 못했습니다.");
        }
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
//using System.Security.Cryptography;
//using UnityEngine;

//public class FireBall : MonoBehaviour
//{
//    public float speed = 3;
//    public Collider2D frontCollider;
//    public EdgeCollider2D terrainCollider;

//    Vector2 vx;
//    void Start()
//    {
//        vx = Vector2.right * speed;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (frontCollider.IsTouching(terrainCollider)) { 
//           vx = -vx;
//            transform.localScale = new Vector2(-transform.localScale.x, 1);
//       }
//    }



//    private void FixedUpdate()
//    {
//        transform.Translate(vx * Time.fixedDeltaTime);
//    }
//}
