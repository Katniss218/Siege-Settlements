using SS.Levels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.UI
{
	public class SkirmishButton : MonoBehaviour
	{
		public void Click()
		{
			LevelManager.Load( Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Levels" + System.IO.Path.DirectorySeparatorChar + "Tutorial" );
		}
	}
}