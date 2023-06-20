using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class which manages the game
/// </summary>
public class GameManager : MonoBehaviour
{
    // The global instance for other scripts to reference
    public static GameManager instance = null;

    [Header("References:")]
    [Tooltip("The UIManager component which manages the current scene's UI")]
    public UIManager uiManager = null;
    [Tooltip("The player gameobject")]
    public GameObject player = null;

    [Header("Scores")]
    [Tooltip("The player's score")]
    [SerializeField] private int gameManagerScore = 0;

    // Static getter/setter for player score (for convenience)
    public static int score
    {
        get
        {
            return instance.gameManagerScore;
        }
        set
        {
            instance.gameManagerScore = value;
        }
    }

    [Tooltip("The highest score acheived on this device")]
    public int highScore = 0;

    [Header("Game Progress / Victory Settings")]
    [Tooltip("Whether the game is winnable or not \nDefault: true")]
    public bool gameIsWinnable = true;
    [Tooltip("Page index in the UIManager to go to on winning the game")]
    public int gameVictoryPageIndex = 0;
    [Tooltip("The effect to create upon winning the game")]
    public GameObject victoryEffect;

    [Header("Player Prefs Settings")]
    public bool resetPlayerPrefsSettings = false;

    /// <summary>
    /// Description:
    /// Standard Unity function called when this instance is first loaded (before start)
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void Awake()
    {
        // When this component is first added or activated, setup the global reference
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destructable.DoDestroy(this.gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first update
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void Start()
    {
        // Less urgent startup behaviors, like loading highscores
        if (PlayerPrefs.HasKey("highscore"))
        {
            highScore = PlayerPrefs.GetInt("highscore");
        }
        if (PlayerPrefs.HasKey("score"))
        {
            score = PlayerPrefs.GetInt("score");
        }
        InitilizeGamePlayerPrefs();
    }

    /// <summary>
    /// Description:
    /// Sets up the game player prefs of the player's health and lives
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void InitilizeGamePlayerPrefs()
    {
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();

            // Set lives accordingly
            if (PlayerPrefs.GetInt("lives") == 0 || resetPlayerPrefsSettings)
            {
                PlayerPrefs.SetInt("lives", playerHealth.currentLives);
            }

            playerHealth.currentLives = PlayerPrefs.GetInt("lives");

            // Set health accordingly
            if (PlayerPrefs.GetInt("health") == 0 || resetPlayerPrefsSettings)
            {
                PlayerPrefs.SetInt("health", playerHealth.currentHealth);
            }

            playerHealth.currentHealth = PlayerPrefs.GetInt("health");

            if (resetPlayerPrefsSettings)
            {
                PlayerPrefs.SetInt("score", 0);
            }
        }
        KeyRing.ClearKeyRing();
    }

    /// <summary>
    /// Description:
    /// Sets the lives and health of the player prefs to the player's lives and health
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void SetGamePlayerPrefs()
    {
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            PlayerPrefs.SetInt("lives", playerHealth.currentLives);
            PlayerPrefs.SetInt("health", playerHealth.currentHealth);
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function that gets called when the application (or playmode) ends
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveHighScore();
        ResetScore();
    }

    /// <summary>
    /// Description:
    /// Sends out a message to UI elements to update
    /// Input:
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public static void UpdateUIElements()
    {
        if (instance != null && instance.uiManager != null)
        {
            instance.uiManager.UpdateUI();
        }
    }

    /// <summary>
    /// Description:
    /// Ends the level, meant to be called when the level is complete (End of level reached)
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public void LevelCleared()
    {
        PlayerPrefs.SetInt("score", score);
        SetGamePlayerPrefs();
        if (uiManager != null)
        {
            // pause the game without brining up the pause screen
            Time.timeScale = 0;
            CursorManager.instance.ChangeCursorMode(CursorManager.CursorState.Menu);
            uiManager.allowPause = false;
            uiManager.GoToPage(gameVictoryPageIndex);
            if (victoryEffect != null)
            {
                Instantiate(victoryEffect, transform.position, transform.rotation, null);
            }
        }
    }

    [Header("Game Over Settings:")]
    [Tooltip("The index in the UI manager of the game over page")]
    public int gameOverPageIndex = 0;
    [Tooltip("The game over effect to create when the game is lost")]
    public GameObject gameOverEffect;

    // Whether or not the game is over
    [HideInInspector]
    public bool gameIsOver = false;

    /// <summary>
    /// Description:
    /// Displays game over screen
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void GameOver()
    {
        gameIsOver = true;
        if (gameOverEffect != null)
        {
            Instantiate(gameOverEffect, transform.position, transform.rotation, null);
        }
        if (uiManager != null)
        {
            // pause the game without brining up the pause screen
            Time.timeScale = 0;
            CursorManager.instance.ChangeCursorMode(CursorManager.CursorState.Menu);
            uiManager.allowPause = false;
            uiManager.GoToPage(gameOverPageIndex);
        }
    }

    /// <summary>
    /// Description:
    /// Adds a number to the player's score stored in the gameManager
    /// Input: 
    /// int scoreAmount
    /// Return: 
    /// void (no return)
    /// </summary>
    /// <param name="scoreAmount">The amount to add to the score</param>
    public static void AddScore(int scoreAmount)
    {
        score += scoreAmount;
        if (score > instance.highScore)
        {
            SaveHighScore();
        }
        UpdateUIElements();
    }

    /// <summary>
    /// Description:
    /// Resets the current player score
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public static void ResetScore()
    {
        PlayerPrefs.SetInt("score", 0);
        score = 0;
    }

    /// <summary>
    /// Description:
    /// Resets the game player prefs of the lives, health, and score
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public static void ResetGamePlayerPrefs()
    {
        PlayerPrefs.SetInt("score", 0);
        score = 0;
        PlayerPrefs.SetInt("lives", 0);
        PlayerPrefs.SetInt("health", 0);
    }

    /// <summary>
    /// Description:
    /// Saves the player's highscore
    /// Input:
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public static void SaveHighScore()
    {
        if (score > instance.highScore)
        {
            PlayerPrefs.SetInt("highscore", score);
            instance.highScore = score;
        }
        UpdateUIElements();
    }

    /// <summary>
    /// Description:
    /// Resets the high score in player preferences
    /// Input:
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public static void ResetHighScore()
    {
        PlayerPrefs.SetInt("highscore", 0);
        if (instance != null)
        {
            instance.highScore = 0;
        }
        UpdateUIElements();
    }
}
