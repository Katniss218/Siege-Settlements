using UnityEngine;

namespace SS.UI
{
	public class ToggleGameObjectAndPause : MonoBehaviour
	{
		[SerializeField] GameObject prefab;

		GameObject toggleGameObject;

		public void _Toggle()
		{
			if( this.toggleGameObject == null )
			{
				if( !PauseManager.isPaused )
				{
					PauseManager.Pause();
				}
				this.toggleGameObject = Object.Instantiate( prefab, Main.canvas.transform );
			}
			else
			{
				if( PauseManager.isPaused )
				{
					PauseManager.Unpause();
				}
				Object.Destroy( toggleGameObject );
			}
		}
	}
}