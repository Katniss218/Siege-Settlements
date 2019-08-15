﻿using UnityEngine;
using UnityEngine.UI;

namespace SS.Units
{
	/// <summary>
	/// Represents a UI, that's attached to a unit (displays health, etc.).
	/// </summary>
	public class UnitUI : MonoBehaviour
	{
		// when the health is 0, the image will be filled this much.
		private const float min = 0.08f;
		// when the health is 1, the image will be filled this much.
		private const float max = 0.67f;

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
			float scale = max - min;
			float p = percentHealth * scale;
			p += min;

			healthBar.fillAmount = p;
		}

		void Awake()
		{
			background = this.GetComponent<Image>();
			healthBar = this.transform.GetChild( 0 ).GetComponent<Image>();
		}
	}
}