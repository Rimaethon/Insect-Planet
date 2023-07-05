using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script is made beacuse of OnValueChange of slider is not working properly all the time due to some bugs.
/// </summary>
public class SliderScript : MonoBehaviour
{

    [SerializeField] Slider slider;
    [SerializeField] Text text;
    
    void Start()
    {
        slider.onValueChanged.AddListener((value)=>text.text = value.ToString());
    }

    
}
