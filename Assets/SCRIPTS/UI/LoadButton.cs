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

			// Buttons will input ids.

			//StartCoroutine( _Load( level, saveState ) );
#warning fill in the ids in the input fields, but if you type in display names, they will get converted to ids.
			//LevelManager.UnloadLevel();
			LevelManager.LoadLevel( levelInput.text, levelSaveStateInput.text );
		}

		IEnumerator _Load( string level, string saveState )
		{
			LevelManager.UnloadLevel();

			yield return null;

			Debug.Log( "BEFORE" );
			LevelManager.LoadLevel( level, saveState );
			Debug.Log( "AFTER" );
#warning doesn't work.
		}
	}
}