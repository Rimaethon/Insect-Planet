using UnityEngine;

namespace Rimaethon._Scripts.Health_Damage
{
	public class EnemyHealth:MonoBehaviour
	{
		public int teamID;
		public int maxHealth = 100;
		public int currentHealth;
		public void TakeDamage(int damage)
		{
			currentHealth -= damage;
			Debug.Log("Took " + damage + " damage");
		}
	}
}
