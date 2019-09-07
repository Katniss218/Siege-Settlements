using UnityEngine;
using UnityEngine.UI;

namespace SS.Units
{
	/// <summary>
	/// Represents a UI, that's attached to a unit (displays health, etc.).
	/// </summary>
	public class ScaledCHUD : MonoBehaviour
	{
		// when the health is 0, the image will be filled this much.
		[SerializeField] private float min = 0.25f;
		// when the health is 1, the image will be filled this much.
		[SerializeField] private float max = 0.75f;

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
			float scale = this.max - this.min;
			float p = percentHealth * scale;
			p += this.min;

			this.healthBar.fillAmount = p;
		}

		void Awake()
		{
			this.resource = this.transform.Find( "Resource" ).GetComponent<Image>();
			this.healthBar = this.transform.Find( "Health Bar" ).GetComponent<Image>();
		}
	}
}