using UnityEngine;
using UnityEngine.UI;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a UI, that's attached to a unit (displays health, etc.).
	/// </summary>
	public class BuildingUI : MonoBehaviour
	{
		private Image background;
		private Image healthBar;

		/// <summary>
		/// Sets the faction color tint to the specified color.
		/// </summary>
		public void SetFactionColor( Color c )
		{
			background.color = c;
			healthBar.color = c;
		}

		/// <summary>
		/// Sets the fill amount of the health bar to the specified value (percent of health).
		/// </summary>
		public void SetHealthFill( float percentHealth )
		{
			healthBar.fillAmount = percentHealth;
		}

		void Awake()
		{
			background = this.GetComponent<Image>();
			healthBar = this.transform.GetChild( 0 ).GetComponent<Image>();
		}
	}
}