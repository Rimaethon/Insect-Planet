using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private bool allowPause = true;
        [SerializeField] private GameObject navigationEffect;
        [SerializeField] private GameObject clickEffect;
        [SerializeField] private GameObject backEffect;

        private bool _isPaused;


        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private InputManager inputManager;

    
        [SerializeField] private Animator initiallyOpen;

        private Animator _activePanelAnimator;
        private Animator _previousPanelAnimator;

        private readonly int _openingParameter = Animator.StringToHash("Open");
        private bool _isInputManagerNull;

        public void OnEnable()
        {

            if (initiallyOpen == null)
                return;

            OpenPanel(initiallyOpen);
            _activePanelAnimator = initiallyOpen;
        
            SetupGameManagerUIManager();
        }

        public void OpenPanel (Animator anim)
        {
            if (_activePanelAnimator == anim)
                return;

            anim.gameObject.SetActive(true);
            _previousPanelAnimator = _activePanelAnimator;
		
            _activePanelAnimator = anim;
            _activePanelAnimator.SetBool(_openingParameter, true);


        }

	
        public void ClosePanel(Animator anim)
        {
            anim.SetBool(_openingParameter,false);
            _activePanelAnimator = _previousPanelAnimator;

        }
        public void CreateBackEffect()
        {
            if (backEffect)
            {
                Instantiate(backEffect, transform.position, Quaternion.identity, null);
            }
        }


        public void CreateClickEffect()
        {
            if (clickEffect)
            {
                Instantiate(clickEffect, transform.position, Quaternion.identity, null);
            }
        }

        public void CreateNavigationEffect()
        {
            if (navigationEffect)
            {
                Instantiate(navigationEffect, transform.position, Quaternion.identity, null);
            }
        }
    


        private void SetupGameManagerUIManager()
        {
            if (GameManager.instance != null && GameManager.instance.uiManager == null)
            {
                GameManager.instance.uiManager = this;
            }
        }


 

  
        private void SetUpEventSystem()
        {
            eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogWarning("There is no event system in the scene.");
            }
        }

   
        private void SetUpInputManager()
        {
            if (inputManager == null)
            {
                inputManager = InputManager.instance;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("The UIManager is missing a reference to an Input Manager.");
            }
        }

    
        public void TogglePause()
        {
            if (allowPause)
            {
                if (_isPaused)
                {
                    CursorManager.instance.ChangeCursorMode(CursorManager.CursorState.FPS);
                    Time.timeScale = 1;
                    _isPaused = false;
                }
                else
                {
                    CursorManager.instance.ChangeCursorMode(CursorManager.CursorState.Menu);
                    Time.timeScale = 0;
                    _isPaused = true;
                }
            }
        }

    
    

   
        private void Start()
        {
            _isInputManagerNull = inputManager == null;
            SetUpInputManager();
            SetUpEventSystem();
        }

 
   
        private void Update()
        {
            CheckPauseInput();
        }

 
        private void CheckPauseInput()
        {
            if (_isInputManagerNull) return;
            if (inputManager.pausePressed)
            {
                TogglePause();
            }
        }
  
   

    

  
    
    }
}
