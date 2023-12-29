using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFadeAnimation : MonoBehaviour
{
    private RectTransform _rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        StartCoroutine(ShakeText());
    }

    // Update is called once per frame
    private void FixedUpdate()
    {


    }
    
    private IEnumerator ShakeText()
    {
       
        while (true)
        {
            _rectTransform.pivot += new Vector2(0.1f,0.1f);
            yield return new WaitForSeconds(0.001f);
            _rectTransform.pivot -= new Vector2(0.1f,0.1f);
            yield return new WaitForSeconds(0.001f);
        }
    }

    void Update()
    {
     
    }
}
