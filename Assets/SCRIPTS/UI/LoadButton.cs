using SS.Levels;
using TMPro;
using UnityEngine;

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

#warning User will input level names (for both saving and loading).
#warning Buttons will input names.

#warning the system will go through every button/something and check the corresponding path.

			if( LevelManager.isLevelLoaded )
			{
				LevelManager.UnloadLevel( false, () =>
				{
					LevelManager.LoadLevel( levelInput.text, levelSaveStateInput.text, null );
				} );
			}
			else
			{
				LevelManager.LoadLevel( levelInput.text, levelSaveStateInput.text, null );
			}
		}
	}
}