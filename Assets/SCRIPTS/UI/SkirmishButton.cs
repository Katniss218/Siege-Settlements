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
			LevelManager.LoadLevel( "Tutorial", LevelManager.DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER );
		}
	}
}