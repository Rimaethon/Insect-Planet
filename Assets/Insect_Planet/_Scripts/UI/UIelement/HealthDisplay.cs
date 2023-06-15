using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class intended to work with grid layout groups to create an image based health bar
/// </summary>
public class HealthDisplay : UIelement
{
    [Header("Settings")]
    [Tooltip("The health component to display health values for")]
    public Health targetHealth = null;
    [Tooltip("The image which represents one unit of health")]
    public GameObject healthDisplayImage = null;
    [Tooltip("The prefab to use to display the number")]
    public GameObject numberDisplay = null;
    [Tooltip("The maximum number of images to display before switching to just a number")]
    public int maximumNumberToDisplay = 3;

    private void Start()
    {
        if (targetHealth == null && (GameManager.instance != null && GameManager.instance.player != null))
        {
            targetHealth = GameManager.instance.player.GetComponentInChildren<Health>();
        }
        UpdateUI();
    }

    /// <summary>
    /// Description:
    /// Upadates this UI element
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public override void UpdateUI()
    {
        //if (GameManager.instance != null && GameManager.instance.player != null)
        //{
        //    Health playerHealth = GameManager.instance.player.GetComponent<Health>();
        //    if (playerHealth != null)
        //    {
        //        SetChildImageNumber(playerHealth.currentHealth);
        //    }
        //}
        if (targetHealth != null)
        {
            SetChildImageNumber(targetHealth.currentHealth);
        }
    }

    /// <summary>
    /// Description:
    /// Deletes and spawns images until this gameobject has as many children as the player has health
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

        if (healthDisplayImage != null)
        {
            if (maximumNumberToDisplay >= number)
            {
                for (int i = 0; i < number; i++)
                {
                    Instantiate(healthDisplayImage, transform);
                }
            }
            else
            {
                Instantiate(healthDisplayImage, transform);
                if (numberDisplay != null)
                {
                    GameObject createdNumberDisp = Instantiate(numberDisplay, transform);
                    createdNumberDisp.GetComponent<Text>().text = number.ToString();
                }
            }
        }
    }
}
