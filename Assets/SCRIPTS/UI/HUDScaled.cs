using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	/// <summary>
	/// Represents a UI, that's attached to a unit (displays health, etc.).
	/// </summary>
	public class HUDScaled : MonoBehaviour, IHUD
	{
		// when the health is 0, the image will be filled this much.
		[SerializeField] private float min = 0.25f;
		// when the health is 1, the image will be filled this much.
		[SerializeField] private float max = 0.75f;

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
			float scale = this.max - this.min;
			float p = percentHealth * scale;
			p += this.min;

			this.healthBar.fillAmount = p;
		}
	}
}