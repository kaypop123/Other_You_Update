using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float speed = 3;
    public Collider2D frontCollider;
    public EdgeCollider2D terrainCollider;

    Vector2 vx;
    void Start()
    {
        vx = Vector2.right * speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (frontCollider.IsTouching(terrainCollider)) { 
           vx = -vx;
            transform.localScale = new Vector2(-transform.localScale.x, 1);
       }
    }

    

    private void FixedUpdate()
    {
        transform.Translate(vx * Time.fixedDeltaTime);
    }
}
