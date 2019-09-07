using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SS.UI
{
	public class ExitButton : MonoBehaviour
	{
		public void QuitGame()
		{
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
		}
	}
}