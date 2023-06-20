using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class which tracks the amount of ammo that belongs to the player.
/// </summary>
public class AmmoTracker : MonoBehaviour
{
    #region Variables
    // Instance of this class which acts as a singleton
    public static AmmoTracker _instance = null;

    // Dictionary containing all of the ammo stored by this ammo tracker
    private Dictionary<int, int> _ammo = new System.Collections.Generic.Dictionary<int, int>();

    // Accessor for the dictionary storing ammo.
    public int this[int ammoID]
    {
        get
        {
            if (!_ammo.ContainsKey(ammoID))
            {
                _ammo.Add(ammoID, 0);
            }
            return _ammo[ammoID];
        }
        set
        {
            if (!_ammo.ContainsKey(ammoID))
            {
                _ammo.Add(ammoID, 0);
            }
            _ammo[ammoID] = value;
        }
    }

    [Tooltip("Whether this tracker saves and loads ammo data or is contained to this scene")]
    public bool isPersistent = true;

    #region Constant Variables
    // The string to concatenate ammo IDs with to get/set ammo values stored in player prefs
    public const string AMMOPLAYERPREFSSTRING = "AmmoID";
    // The string to use to save/load the ammo types stored by player prefs
    public const string ALLSAVEDAMMOPREFSSTRING = "AllAmmo";
    // The maximum amount of ammo of a single type that the player can hold at once.
    public const int MAXAMMO = 100;
    #endregion
    #endregion

    #region Functions
    #region GameObject Functions
    /// <summary>
    /// Description:
    /// When this script starts up, do setup work
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    private void Awake()
    {
        SetupAsSingleton();
        LoadStoredAmmo();
    }

    /// <summary>
    /// Description:
    /// When the application stops playing, save ammo values to player prefs
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveStoredAmmo();
    }

    private void OnSceneUnLoaded(UnityEngine.SceneManagement.Scene scene)
    {
        SaveStoredAmmo();
    }
    #endregion

    #region Singleton Behavior
    /// <summary>
    /// Description:
    /// Sets up this gameobject as a singleton and sets it as AmmoTracker._instance
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    private void SetupAsSingleton()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        transform.parent = null;
        SceneManager.sceneUnloaded += OnSceneUnLoaded;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Saving & Loading
    /// <summary>
    /// Description:
    /// Saves ammo to player prefs
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    public static void SaveStoredAmmo()
    {
        if (_instance != null && _instance.isPersistent)
        {
            List<int> storedAmmoIDs = new List<int>();
            foreach (var keyValPair in _instance._ammo)
            {
                string prefName = AMMOPLAYERPREFSSTRING + keyValPair.Key.ToString();
                PlayerPrefs.SetInt(prefName, keyValPair.Value);
                storedAmmoIDs.Add(keyValPair.Key);
            }
            PlayerPrefs.SetString(ALLSAVEDAMMOPREFSSTRING, string.Join(",", storedAmmoIDs.ToArray()));
        }
    }

    /// <summary>
    /// Description:
    /// Loads ammo values from player prefs
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    public static void LoadStoredAmmo()
    {
        if (_instance != null && _instance.isPersistent)
        {
            if (PlayerPrefs.HasKey(ALLSAVEDAMMOPREFSSTRING))
            {
                List<string> storedAmmoIDStrings = PlayerPrefs.GetString(ALLSAVEDAMMOPREFSSTRING).Split(',').ToList();
                foreach (string storedAmmoIDstring in storedAmmoIDStrings)
                {
                    string prefName = AMMOPLAYERPREFSSTRING + storedAmmoIDstring;
                    int ammo = PlayerPrefs.GetInt(prefName);
                    string ammoIDString = "0" + prefName.Substring(AMMOPLAYERPREFSSTRING.Length);
                    int ammoID = int.Parse(ammoIDString);
                    _instance._ammo[ammoID] = ammo;
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// Description:
    /// Determines whether the player has ammo which works with the parameter gun
    /// Inputs: Gun gun
    /// Outputs: bool
    /// </summary>
    /// <param name="gun">The gun to test for ammo with</param>
    /// <returns>Whether there is ammo that works with the gun</returns>
    public static bool HasAmmo(Gun gun)
    {
        if (_instance != null && gun != null)
        {
            return _instance[gun.ammunitionID] > 0;
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// Supplies the parameter gun with ammo
    /// Inputs: Gun gun
    /// Outputs: int
    /// </summary>
    /// <param name="gun">The gun to reload</param>
    /// <returns>The ammount of ammo placed in the gun</returns>
    public static int Reload(Gun gun)
    {
        if (_instance != null && gun != null)
        {
            int amountToReload = Mathf.Clamp(_instance[gun.ammunitionID], 0, gun.magazineSize);
            gun.roundsLoaded = amountToReload;
            return amountToReload;
        }
        GameManager.UpdateUIElements();
        return 0;
    }

    /// <summary>
    /// Description:
    /// Should be called when a gun fires to decrement the ammount of ammunition for it
    /// Inputs: Gun gun
    /// Outputs: N/A
    /// </summary>
    /// <param name="gun">The gun that is being fired</param>
    public static void OnFire(Gun gun)
    {
        if (_instance != null && gun != null)
        {
            _instance[gun.ammunitionID] = Mathf.Clamp(_instance[gun.ammunitionID] - (gun.useAmmo ? 1 : 0), 0, MAXAMMO);
        }
        GameManager.UpdateUIElements();
    }

    /// <summary>
    /// Description:
    /// Adds ammunition to the player's reserves
    /// Inputs: int ammoID, int amount
    /// Outputs: N/A
    /// </summary>
    /// <param name="ammoID">The ID of ammunition to add</param>
    /// <param name="amount">The ammount of ammunition to add</param>
    public static void AddAmmunition(int ammoID, int amount)
    {
        if (_instance != null)
        {
            _instance[ammoID] = Mathf.Clamp((_instance[ammoID] + amount), 0, MAXAMMO);
        }
        GameManager.UpdateUIElements();
    }
    #endregion
}
