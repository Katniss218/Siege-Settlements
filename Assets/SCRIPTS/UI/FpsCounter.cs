﻿using TMPro;
using UnityEngine;

namespace SS.UI
{
	/// <summary>
	/// Counts the FPS and displays that to a text field (TextMesh Pro).
	/// </summary>
	public class FpsCounter : MonoBehaviour
	{
		[SerializeField] private TMP_Text textField = null;

		public string format = "Fps: {0}";

		private int GetFps()
		{
			return Mathf.CeilToInt( 1.0f / Time.deltaTime );
		}

		void Update()
		{
			this.textField.text = string.Format( this.format, this.GetFps() );
		}
	}
}