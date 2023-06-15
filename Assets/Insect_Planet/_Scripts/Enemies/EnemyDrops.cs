using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class that handles spawning of prefabs based on random chances
/// </summary>
public class EnemyDrops : MonoBehaviour
{
    [Tooltip("The number of items to spawn")]
    public int numberOfDrops = 1;
    [Tooltip("The prefabs which can be spawned by this script")]
    public List<GameObject> drops = new List<GameObject>();
    [Tooltip("The percentage probability that each drop will spawn. \n" +
        "MUST BE THE SAME LENGTH AS THE LIST OF DROPS.")]
    public List<float> dropPercentages = new List<float>();

    /// <summary>
    /// Description:
    /// When this script starts up, spawn drops
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    private void Start()
    {
        SpawnRandomDrop();
    }

    /// <summary>
    /// Description:
    /// Spawn random drops based on drop percentages
    /// Inputs: N/A
    /// Outputs: N/A
    /// Disclaimer:
    /// The drop percentages are approximate and may not reflect in-game behavior perfectly,
    /// since the "correct" way to implement the randomization is more complex than what
    /// happens in this function.
    /// </summary>
    public void SpawnRandomDrop()
    {
        if (drops.Count == dropPercentages.Count)
        {
            for (int i = 0; i < numberOfDrops; i++)
            {
                GameObject selectedDrop = null;
                List<(GameObject, float)> sortableList = drops.Zip(dropPercentages, (a, b) => { return (a, b); }).ToList();
                sortableList.Sort((a, b) => { return a.Item2.CompareTo(b.Item2); });
                for (int j = sortableList.Count - 1; j >= 0; j--)
                {
                    float rand = Random.value;
                    if (sortableList[j].Item2 >= rand)
                    {
                        selectedDrop = sortableList[j].Item1;
                        break;
                    }
                }
                if (selectedDrop != null)
                {
                    GameObject drop = Instantiate(selectedDrop, transform.position, transform.rotation, null);
                }
            }
        }
        else
        {
            Debug.LogError("Drops percentage list and drops list do not contain the same number of items!");
        }
    }
}
