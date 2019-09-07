using UnityEngine;
using UnityEngine.UI;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a UI, that's attached to a unit (displays health, etc.).
	/// </summary>
	public class UnscaledCHUD : MonoBehaviour
	{
		private Image resource;
		private Image healthBar;

		/// <summary>
		/// Sets the faction color tint to the specified color.
		/// </summary>
		public void SetFactionColor( Color c )
		{
			this.resource.color = c;
			this.healthBar.color = c;
		}

		/// <summary>
		/// Sets the fill amount of the health bar to the specified value (percent of health).
		/// </summary>
		public void SetHealthFill( float percentHealth )
		{
			this.healthBar.fillAmount = percentHealth;
		}

		void Awake()
		{
			this.resource = this.transform.Find( "Resource" ).GetComponent<Image>();
			this.healthBar = this.transform.Find( "Health Bar" ).GetComponent<Image>();
		}
	}
}