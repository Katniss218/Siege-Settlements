using UnityEngine;

namespace SS.UI
{
	public class ToggleGameObjectAndPause : MonoBehaviour
	{
		[SerializeField] GameObject toggleGameObject = null;
		
		public void _Toggle()
		{
			if( this.toggleGameObject.activeSelf )
			{
				if( PauseManager.isPaused )
				{
					PauseManager.Unpause();
				}
				toggleGameObject.SetActive( false );
			}
			else
			{
				if( !PauseManager.isPaused )
				{
					PauseManager.Pause();
				}
				toggleGameObject.SetActive( true );
			}
		}
	}
}