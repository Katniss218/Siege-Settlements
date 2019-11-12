using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class HUDScaled : HUD
	{
		/// <summary>
		/// when the health is 0, the image will be filled this much.
		/// </summary>
		public float min = 0.25f;
		/// <summary>
		/// when the health is 1, the image will be filled this much.
		/// </summary>
		public float max = 0.75f;

		/// <summary>
		/// The list of Image components that are affected by faction color.
		/// </summary>
		public Image[] colored;

		/// <summary>
		/// The Image component that displays current health percent.
		/// </summary>
		public Image healthBar;
		

		/// <summary>
		/// Colors the specified image components with a given faction color.
		/// </summary>
		public override void SetColor( Color c )
		{
			for( int i = 0; i < colored.Length; i++ )
			{
				colored[i].color = c;
			}
		}

		/// <summary>
		/// Sets the fill amount of the health bar to the specified value (percent of health remaining).
		/// </summary>
		public override void SetHealthBarFill( float percentHealth )
		{
			float scale = this.max - this.min;
			float p = percentHealth * scale;
			p += this.min;

			this.healthBar.fillAmount = p;
		}
	}
}