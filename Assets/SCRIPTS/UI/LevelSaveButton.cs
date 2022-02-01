using SS.Levels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SS.UI
{
	public class LevelSaveButton : MonoBehaviour
	{
		[SerializeField] private SaveLoadMenu saveLoadMenu = null;

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
			if( levelInput.text != LevelManager.currentLevelId )
			{
				Debug.LogWarning( "Tried to save a level save state of '" + LevelManager.currentLevelId + "' as a level save state of '" + levelInput.text + "'." );
				return;
			}

			LevelManager.SaveScene( levelSaveStateInput.text, levelSaveStateInput.text );

			saveLoadMenu.ForceRefresh();
		}
	}
}