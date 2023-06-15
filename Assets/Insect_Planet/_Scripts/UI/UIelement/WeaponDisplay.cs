using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles updating the high score display
/// </summary>
public class WeaponDisplay : UIelement
{
    public Text ammoText = null;
    public RawImage gunDisplayImage;
    public RawImage ammoPackDisplayImage;
    public AmmoTracker ammoTracker; 

    public void DisplayGunInformation()
    {
        Shooter playerShooter = GameManager.instance.player.GetComponentInChildren<PlayerController>().playerShooter;

        if (ammoText != null && playerShooter.guns[playerShooter.equippedGunIndex].useAmmo)
        {
            ammoText.text = AmmoTracker._instance[playerShooter.guns[playerShooter.equippedGunIndex].ammunitionID].ToString();
            if (ammoPackDisplayImage != null && playerShooter.guns[playerShooter.equippedGunIndex].ammoImage != null)
            {
                ammoPackDisplayImage.color = new Color(255, 255, 255, 255);
                ammoPackDisplayImage.texture = playerShooter.guns[playerShooter.equippedGunIndex].ammoImage.texture;
            }
        }
        else
        {
            ammoText.text = "";
            ammoPackDisplayImage.color = new Color(0,0,0,0);
        }
        if (playerShooter.guns[playerShooter.equippedGunIndex].weaponImage != null && gunDisplayImage != null)
        {
            gunDisplayImage.texture = playerShooter.guns[playerShooter.equippedGunIndex].weaponImage.texture;
        }
    }

  
    public override void UpdateUI()
    {
        base.UpdateUI();

        DisplayGunInformation();
    }
}