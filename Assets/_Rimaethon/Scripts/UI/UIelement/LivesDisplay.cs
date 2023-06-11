using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class which causes the text component on this gameobject to display the number of lives the player has left
/// </summary>
public class LivesDisplay : UIelement
{
    [Header("Settings")]
    [Tooltip("The prefab image to use when displaying lives remaining")]
    public GameObject livesDisplayImage = null;
    [Tooltip("The prefab to use to display the number")]
    public GameObject numberDisplay = null;
    [Tooltip("The maximum number of images to display before switching to just a number")]
    public int maximumNumberToDisplay = 3;

    /// <summary>
    /// Description:
    /// Updates the display according to this class
    /// When UI elements update, update the display of the number of player lives remaining
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public override void UpdateUI()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            Health playerHealth = GameManager.instance.player.GetComponent<Health>();
            if (playerHealth != null)
            {
                SetChildImageNumber(playerHealth.currentLives - 1);
            }
        }
    }

    /// <summary>
    /// Description:
    /// Deletes and spawns images until this gameobject has as many children as the player has lives
    /// If the number of lives is over the threshold, displays a number instead
    /// Input: 
    /// int
    /// Return: 
    /// void (no return)
    /// </summary>
    /// <param name="number">The number of images that this object should have as children</param>
    private void SetChildImageNumber(int number)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        if (livesDisplayImage != null)
        {
            if (maximumNumberToDisplay >= number)
            {
                for (int i = 0; i < number; i++)
                {
                    Instantiate(livesDisplayImage, transform);
                }
            }
            else
            {
                Instantiate(livesDisplayImage, transform);
                GameObject createdNumberDisp = Instantiate(numberDisplay, transform);
                createdNumberDisp.GetComponent<Text>().text = number.ToString();
            }
        }
    }
}
