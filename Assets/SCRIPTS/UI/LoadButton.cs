using SS.Levels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SS.UI
{
	public class LoadButton : MonoBehaviour
	{
		public TMP_InputField levelInput;
		public TMP_InputField levelSaveStateInput;

		public void _Trigger()
		{
			// If the input fields are not filled - can't load anything.
			if( string.IsNullOrEmpty( levelInput.text ) )
			{
				return;
			}
			if( string.IsNullOrEmpty( levelSaveStateInput.text ) )
			{
				return;
			}

			// User will input level names (for both saving and loading).
			// Buttons will input names.

			// the system will go through every button/something and check the corresponding path.

			if( LevelManager.isLevelLoaded )
			{
#warning figure out a way to load levels from within the in-game save/load menu.
				throw new System.Exception( "The level is already loaded." );
			}
			LevelManager.LoadLevel( levelInput.text, levelSaveStateInput.text );
		}
	}
}