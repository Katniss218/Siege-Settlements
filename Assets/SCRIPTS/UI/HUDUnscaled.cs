using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class HUDUnscaled : HUD
	{
		/// <summary>
		/// The list of Image components that are affected by faction color.
		/// </summary>
		public Image[] colored;

		/// <summary>
		/// The Image component that displays current health percent.
		/// </summary>
		public Image healthBar;

		/// <summary>
		/// Sets the faction color tint to the specified color.
		/// </summary>
		public override void SetColor( Color c )
		{
			for( int i = 0; i < colored.Length; i++ )
			{
				colored[i].color = c;
			}
		}

		/// <summary>
		/// Sets the fill amount of the health bar to the specified value (percent of health).
		/// </summary>
		public override void SetHealthBarFill( float percentHealth )
		{
			this.healthBar.fillAmount = percentHealth;
		}
	}
}