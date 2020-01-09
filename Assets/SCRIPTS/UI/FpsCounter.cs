using TMPro;
using UnityEngine;

namespace SS.UI
{
	/// <summary>
	/// Calculates the FPS and displays it in a text field (TextMesh Pro).
	/// </summary>
	public class FpsCounter : MonoBehaviour
	{
		[SerializeField] private TMP_Text textField = null;

		/// <summary>
		/// Tells the FpsCounter to format the text accordingly.
		/// </summary>
		public string format = "Fps: {0}";

		private int GetFps()
		{
			// If is paused, return 0 FPS.
			if( Time.deltaTime == 0 )
			{
				return 0;
			}
			// Otherwise, calculate the real FPS.
			return Mathf.CeilToInt( 1.0f / Time.deltaTime );
		}

		private float samplingInterval = 0.25f;
		private float lastSampleTimestamp;

		private void SampleFPS()
		{
			this.textField.text = string.Format( this.format, this.GetFps() );
			this.lastSampleTimestamp = Time.time;
		}

		void Update()
		{
			if( Time.time > this.lastSampleTimestamp + samplingInterval )
			{
				this.SampleFPS();
			}
		}
	}
}