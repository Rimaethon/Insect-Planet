using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float scaleUpAmount=1.2f
    
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LTDescr tween = LeanTween.scale(gameObject, gameObject.RectTransormscaleUpAmount, scaleDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(UnScaleButton);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
    
    
    private void UnScaleButton()
    {
        // Scale down the button
        LTDescr tween = LeanTween.scale(button.gameObject, Vector3.one, scaleDuration)
            .setEase(LeanTweenType.easeInOutSine);
    }
}

