using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{

    [SerializeField] private List<UIPage> pages;
    public int currentPage = 0;
    public int defaultPage = 0;

    public int pausePageIndex = 1;
    [SerializeField] private bool allowPause = true;
    [SerializeField] private GameObject navigationEffect;
    [SerializeField] private GameObject clickEffect;
    [SerializeField] private GameObject backEffect;

    private bool isPaused = false;

    private List<UIelement> UIelements;

    [HideInInspector]
    public EventSystem eventSystem;
    [SerializeField]
    private InputManager inputManager;

    
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

  
    private void OnEnable()
    {
        SetupGameManagerUIManager();
    }


    private void SetupGameManagerUIManager()
    {
     
    }

 
    private void SetUpUIElements()
    {
        UIelements = FindObjectsOfType<UIelement>().ToList();
    }


    private void SetUpEventSystem()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("There is no event system in the scene but you are trying to use the UIManager. /n" +
                "All UI in Unity requires an Event System to run. /n" +
                "You can add one by right clicking in hierarchy then selecting UI->EventSystem.");
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
            Debug.LogWarning("The UIManager is missing a reference to an Input Manager, without a Input Manager the UI can not pause");
        }
    }


    public void TogglePause()
    {
        if (allowPause)
        {
            if (isPaused)
            {
                CursorManager.instance.ChangeCursorMode(CursorManager.CursorState.FPS);
                GoToPage(defaultPage);
                Time.timeScale = 1;
                isPaused = false;
            }
            else
            {
                CursorManager.instance.ChangeCursorMode(CursorManager.CursorState.Menu);
                GoToPage(pausePageIndex);
                Time.timeScale = 0;
                isPaused = true;
            }
        }
    }

    public void UpdateUI()
    {
        foreach (UIelement uiElement in UIelements)
        {
            uiElement.UpdateUI();
        }
    }


    private void Start()
    {
        SetUpInputManager();
        SetUpEventSystem();
        SetUpUIElements();
        InitilizeFirstPage();
        UpdateUI();
    }

  
    private void InitilizeFirstPage()
    {
        GoToPage(defaultPage);
    }

    private void Update()
    {
        CheckPauseInput();
    }

 
    private void CheckPauseInput()
    {
        if (inputManager != null)
        {
            if (inputManager.pausePressed)
            {
                TogglePause();
            }
        }
    }

    public void GoToPage(int pageIndex)
    {
        if (pageIndex < pages.Count && pages[pageIndex] != null)
        {
            SetActiveAllPages(false);
            pages[pageIndex].gameObject.SetActive(true);
            pages[pageIndex].SetSelectedUIToDefault();
        }
    }

    public void GoToPageByName(string pageName)
    {
        UIPage page = pages.Find(item => item.name == pageName);
        int pageIndex = pages.IndexOf(page);
        GoToPage(pageIndex);
    }

   
    public void SetActiveAllPages(bool activated)
    {
        if (pages != null)
        {
            foreach (UIPage page in pages)
            {
                if (page != null)
                    page.gameObject.SetActive(activated);
            }
        }
    }
}
