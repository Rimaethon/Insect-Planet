using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class localtoworld : MonoBehaviour
{
    [Range(0,1)]
    public float angle=0;

    public float lerpedvalue;
    void Start()
    {

       

    }

    private void Update()
    {
        lerpedvalue=Mathf.Lerp(1, -1, angle);
    }
}
