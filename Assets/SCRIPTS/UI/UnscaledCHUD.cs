using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	/// <summary>
	/// Represents a UI, that's attached to a unit (displays health, etc.).
	/// </summary>
	public class UnscaledCHUD : MonoBehaviour
	{
		public Image[] colored;
		public Image healthBar;

		/// <summary>
		/// Sets the faction color tint to the specified color.
		/// </summary>
		public void SetColor( Color c )
		{
			for( int i = 0; i < colored.Length; i++ )
			{
				colored[i].color = c;
			}
		}

		/// <summary>
		/// Sets the fill amount of the health bar to the specified value (percent of health).
		/// </summary>
		public void SetHealthBarFill( float percentHealth )
		{
			this.healthBar.fillAmount = percentHealth;
		}
	}
}