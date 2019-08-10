using TMPro;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Counts the FPS and displays that to a text field (TextMesh Pro).
	/// </summary>
	public class FPSCounter : MonoBehaviour
	{
		[SerializeField] private TMP_Text textField = null;

		[SerializeField] private string format = "FPS: {0}";

		void Update()
		{
			int fps = Mathf.CeilToInt( 1.0f / Time.deltaTime );

			textField.text = string.Format( format, fps );
		}
	}
}