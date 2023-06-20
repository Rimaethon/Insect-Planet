using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// A class which manages pages of UI elements
/// and the game's UI
/// </summary>
public class UIManager : MonoBehaviour
{

    [Header("Page Management")]
    [Tooltip("The pages (Panels) managed by the UI Manager")]
    public List<UIPage> pages;
    [Tooltip("The index of the active page in the UI")]
    public int currentPage = 0;
    [Tooltip("The page (by index) switched to when the UI Manager starts up")]
    public int defaultPage = 0;

    [Header("Pause Settings")]
    [Tooltip("The index of the pause page in the pages list")]
    public int pausePageIndex = 1;
    [Tooltip("Whether or not to allow pausing")]
    public bool allowPause = true;
    [Header("Polish Effects")]
    [Tooltip("The effect to create when navigating between UI")]
    public GameObject navigationEffect;
    [Tooltip("The effect to create when clicking on or pressing a UI element")]
    public GameObject clickEffect;
    [Tooltip("The effect to create when the player is backing out of a Menu page")]
    public GameObject backEffect;

    // Whether the application is paused
    private bool isPaused = false;

    // A list of all UI element classes
    private List<UIelement> UIelements;

    // The event system handling UI navigation
    [HideInInspector]
    public EventSystem eventSystem;
    // The Input Manager to listen for pausing
    [SerializeField]
    private InputManager inputManager;

    /// <summary>
    /// Description:
    /// Creates a back effect if one is set
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void CreateBackEffect()
    {
        if (backEffect)
        {
            Instantiate(backEffect, transform.position, Quaternion.identity, null);
        }
    }

    /// <summary>
    /// Description:
    /// Creates a click effect if one is set
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void CreateClickEffect()
    {
        if (clickEffect)
        {
            Instantiate(clickEffect, transform.position, Quaternion.identity, null);
        }
    }

    /// <summary>
    /// Description:
    /// Creates a navigation effect if one is set
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void CreateNavigationEffect()
    {
        if (navigationEffect)
        {
            Instantiate(navigationEffect, transform.position, Quaternion.identity, null);
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called when the attached gameobject becomes enabled
    /// When this component wakes up (including switching scenes) it sets itself as the GameManager's UI manager
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void OnEnable()
    {
        SetupGameManagerUIManager();
    }

    /// <summary>
    /// Description:
    /// Sets this component as the UI manager for the GameManager
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void SetupGameManagerUIManager()
    {
        if (GameManager.instance != null && GameManager.instance.uiManager == null)
        {
            GameManager.instance.uiManager = this;
        }
    }

    /// <summary>
    /// Description:
    /// Finds and stores all UIElements in the UIElements list
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void SetUpUIElements()
    {
        UIelements = FindObjectsOfType<UIelement>().ToList();
    }

    /// <summary>
    /// Description:
    /// Gets the event system from the scene if one exists
    /// If one does not exist a warning will be displayed
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
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

    /// <summary>
    /// Description:
    /// Attempts to set up an input manager with this UI so manager so it can get pause input
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
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

    /// <summary>
    /// Description:
    /// If the game is paused, unpauses the game.
    /// If the game is not paused, pauses the game.
    /// Input:
    /// none
    /// Retun:
    /// void (no return)
    /// </summary>
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

    /// <summary>
    /// Description:
    /// Goes through all UI elements and calls their UpdateUI function
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void UpdateUI()
    {
        foreach (UIelement uiElement in UIelements)
        {
            uiElement.UpdateUI();
        }
    }

    /// <summary>
    /// Description:
    /// Default function from Unity that runs when the script is first started
    /// Input: 
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void Start()
    {
        SetUpInputManager();
        SetUpEventSystem();
        SetUpUIElements();
        InitilizeFirstPage();
        UpdateUI();
    }

    /// <summary>
    /// Description:
    /// Sets up the first page
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void InitilizeFirstPage()
    {
        GoToPage(defaultPage);
    }

    /// <summary>
    /// Description:
    /// Default function from Unity that runs every frame
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void Update()
    {
        CheckPauseInput();
    }

    /// <summary>
    /// Description:
    /// If the input manager is set up, reads the pause input
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
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
    /// <summary>
    /// Description:
    /// Goes to a page by that page's index
    /// Input: 
    /// int pageIndex
    /// Return: 
    /// void (no return)
    /// </summary>
    /// <param name="pageIndex">The index in the page list to go to</param>
    public void GoToPage(int pageIndex)
    {
        if (pageIndex < pages.Count && pages[pageIndex] != null)
        {
            SetActiveAllPages(false);
            pages[pageIndex].gameObject.SetActive(true);
            pages[pageIndex].SetSelectedUIToDefault();
        }
    }

    /// <summary>
    /// Description:
    /// Goes to a page by that page's name
    /// Input: 
    /// int pageName
    /// Return: 
    /// void (no return)
    /// </summary>
    /// <param name="pageName">The name of the page in the game you want to go to, if their are duplicates this picks the first found</param>
    public void GoToPageByName(string pageName)
    {
        UIPage page = pages.Find(item => item.name == pageName);
        int pageIndex = pages.IndexOf(page);
        GoToPage(pageIndex);
    }

    /// <summary>
    /// Description:
    /// Turns all stored pages on or off depending on parameters
    /// Input: 
    /// bool activated
    /// Return: 
    /// void (no return)
    /// </summary>
    /// <param name="activated">The true or false value to set all page game objects activeness to</param>
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
