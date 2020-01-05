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
			
			if( LevelManager.isLevelLoaded )
			{
				LevelManager.UnloadLevel( false, () =>
				{
					LevelManager.LoadLevel( levelInput.text, levelSaveStateInput.text );
				} );
			}
			else
			{
				LevelManager.LoadLevel( levelInput.text, levelSaveStateInput.text );
			}
		}
	}
}