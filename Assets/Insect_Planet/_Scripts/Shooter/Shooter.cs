using System.Collections.Generic;
using System.Linq;
using Insect_Planet.Scripts.Shooter;
using UnityEngine;


public class Shooter : MonoBehaviour
{
    public List<Gun> guns = new List<Gun>();
    public int equippedGunIndex = 0;
    public InputManager inputManager;
    public bool isPlayerControlled = false;

   
    void Start()
    {
        SetUpInput();
        SetUpGuns();
    }

   
    void Update()
    {
        CheckInput();
    }

   
    void CheckInput()
    {
        if (!isPlayerControlled)
        {
            return;
        }

        if (Time.timeScale == 0)
        {
            return;
        }

        if (guns.Count > 0)
        {
            if (guns[equippedGunIndex].fireType == Gun.FireType.SemiAutomatic)
            {
                if (inputManager.firePressed)
                {
                    FireEquippedGun();
                }
            }
            else if (guns[equippedGunIndex].fireType == Gun.FireType.Automatic)
            {
                if (inputManager.firePressed || inputManager.fireHeld)
                {
                    FireEquippedGun();
                }
            }

            if (inputManager.cycleWeaponInput != 0)
            {
                CycleEquippedGun();
            }

            if (inputManager.nextWeaponPressed)
            {
                GoToNextWeapon();
            }

            if (inputManager.previousWeaponPressed)
            {
                GoToPreviousWeapon();
            }
        }
    }


    private void GoToNextWeapon()
    {
        List<Gun> availableGuns = guns.Where(item => item.available == true).ToList();
        int maximumAvailableGunIndex = availableGuns.Count - 1;
        int equippedAvailableGunIndex = availableGuns.IndexOf(guns[equippedGunIndex]);

        equippedAvailableGunIndex += 1;
        if (equippedAvailableGunIndex > maximumAvailableGunIndex)
        {
            equippedAvailableGunIndex = 0;
        }

        EquipGun(guns.IndexOf(availableGuns[equippedAvailableGunIndex]));
    }


    private void GoToPreviousWeapon()
    {
        List<Gun> availableGuns = guns.Where(item => item.available == true).ToList();
        int maximumAvailableGunIndex = availableGuns.Count - 1;
        int equippedAvailableGunIndex = availableGuns.IndexOf(guns[equippedGunIndex]);

        equippedAvailableGunIndex -= 1;
        if (equippedAvailableGunIndex < 0)
        {
            equippedAvailableGunIndex = maximumAvailableGunIndex;
        }

        EquipGun(guns.IndexOf(availableGuns[equippedAvailableGunIndex]));
    }

 
    void CycleEquippedGun()
    {
        float cycleInput = inputManager.cycleWeaponInput;
        List<Gun> availableGuns = guns.Where(item => item.available == true).ToList();
        int maximumAvailableGunIndex = availableGuns.Count - 1;
        int equippedAvailableGunIndex = availableGuns.IndexOf(guns[equippedGunIndex]);
        if (cycleInput < 0)
        {
            equippedAvailableGunIndex += 1;
            if (equippedAvailableGunIndex > maximumAvailableGunIndex)
            {
                equippedAvailableGunIndex = 0;
            }
        }
        else if (cycleInput > 0)
        {
            equippedAvailableGunIndex -= 1;
            if (equippedAvailableGunIndex < 0)
            {
                equippedAvailableGunIndex = maximumAvailableGunIndex;
            }
        }

        EquipGun(guns.IndexOf(availableGuns[equippedAvailableGunIndex]));
    }


    private void EquipGun(int gunIndex)
    {
        equippedGunIndex = gunIndex;
        guns[equippedGunIndex].gameObject.SetActive(true);
        for (int i = 0; i < guns.Count; i++)
        {
            if (equippedGunIndex != i)
            {
                guns[i].gameObject.SetActive(false);
            }
        }
        GameManager.UpdateUIElements();
    }


    void SetUpGuns()
    {
        foreach(Gun gun in guns)
        {
            if (gun != null)
            {
                if (gun.available && guns[equippedGunIndex] == gun)
                {
                    gun.gameObject.SetActive(true);
                }
                else
                {
                    gun.gameObject.SetActive(false);
                }
            }
        }
    }

    void SetUpInput()
    {
        if (inputManager == null)
        {
            inputManager = FindObjectOfType<InputManager>();
        }
        if (inputManager == null)
        {
            Debug.LogError("There is no input manager in the scene, the shooter script requires an input manager in order to work for the player");
        }
    }

  
    public void FireEquippedGun()
    {
        if (guns[equippedGunIndex] != null && guns[equippedGunIndex].available)
        {
            guns[equippedGunIndex].Fire();
        }   
    }

 
    public void MakeGunAvailable(int gunIndex)
    {
        if (gunIndex < guns.Count && guns[gunIndex] != null && guns[gunIndex].available == false)
        {
            guns[gunIndex].available = true;
            EquipGun(gunIndex);
        }
    }
}
