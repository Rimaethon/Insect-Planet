using System.Collections.Generic;
using System.Linq;
using Insect_Planet.Scripts.Shooter;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AmmoTracker : MonoBehaviour
{
    #region Variables
    public static AmmoTracker _instance = null;

    private Dictionary<int, int> _ammo = new System.Collections.Generic.Dictionary<int, int>();

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
    private const string AMMOPLAYERPREFSSTRING = "AmmoID";
    private const string ALLSAVEDAMMOPREFSSTRING = "AllAmmo";
    private const int MAXAMMO = 100;
    #endregion
    #endregion

    #region Functions
    #region GameObject Functions
 
    private void Awake()
    {
        SetupAsSingleton();
        LoadStoredAmmo();
    }

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

  
    public static bool HasAmmo(Gun gun)
    {
        if (_instance != null && gun != null)
        {
            return _instance[gun.ammunitionID] > 0;
        }
        return false;
    }


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

    public static void OnFire(Gun gun)
    {
        if (_instance != null && gun != null)
        {
            _instance[gun.ammunitionID] = Mathf.Clamp(_instance[gun.ammunitionID] - (gun.useAmmo ? 1 : 0), 0, MAXAMMO);
        }
        GameManager.UpdateUIElements();
    }


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
