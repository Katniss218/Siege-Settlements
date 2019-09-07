using TMPro;
using UnityEngine;

namespace SS.UI
{
	/// <summary>
	/// Counts the FPS and displays that to a text field (TextMesh Pro).
	/// </summary>
	public class FPSDisplay : MonoBehaviour
	{
		[SerializeField] private TMP_Text textField = null;

		public string format = "FPS: {0}";

		private int GetFps()
		{
			return Mathf.CeilToInt( 1.0f / Time.deltaTime );
		}

		void Update()
		{
			textField.text = string.Format( format, GetFps() );
		}
	}
}