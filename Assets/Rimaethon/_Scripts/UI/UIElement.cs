using Rimaethon._Scripts.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rimaethon._Scripts.UI
{
    public abstract class UIElement : MonoBehaviour
    {
         
         

         public void OpenPage()
         {
             UIAnimationHelperMethods.SlideIn(gameObject.GetComponent<RectTransform>(), UIDirection.UP, 0.5f,
                 null);
         }

         public void OnPageRequest()
        {
            EventManager.Instance.Broadcast(GameEvents.OnPageChange,gameObject);
            
        }
        
        
    }
    
  
    
}
