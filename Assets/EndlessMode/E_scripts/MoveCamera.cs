using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{

    public Transform adam;
    public Transform deva;
    void Start()
    {
        
    }

    void LateUpdate()
    {
        transform.position = new Vector3(adam.position.x, adam.position.y, -10f);
        transform.position = new Vector3(deva.position.x, deva.position.y, -10f);
    }
}
