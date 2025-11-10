using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdamIsCasting : MonoBehaviour
{

    public bool IsCasting = false;    // Start is called before the first frame update



    public void SetCastingTrue()
    {
        IsCasting = true;
    }

    public void SetCastingFalse()
    {
        IsCasting = false;
    }
}
