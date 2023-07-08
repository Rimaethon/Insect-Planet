using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     This script is made beacuse of OnValueChange of slider is not working properly all the time due to some bugs.
/// </summary>
public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Text text;

    private void Start()
    {
        slider.onValueChanged.AddListener(value => text.text = value.ToString());
    }
}