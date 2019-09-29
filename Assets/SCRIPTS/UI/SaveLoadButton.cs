using SS.Content;
using UnityEngine;

namespace SS.UI
{
	public class SaveLoadButton : MonoBehaviour
	{
		GameObject saveLoadMenu;

		public void _Toggle()
		{
			if( this.saveLoadMenu == null )
			{
				if( !PauseManager.isPaused )
				{
					PauseManager.Pause();
				}
				this.saveLoadMenu = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/map Scene/Save-Load Menu" ), Main.canvas.transform );
			}
			else
			{
				if( PauseManager.isPaused )
				{
					PauseManager.Unpause();
				}
				Object.Destroy( saveLoadMenu );
			}
		}
	}
}