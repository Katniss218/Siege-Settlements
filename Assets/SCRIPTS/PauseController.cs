using UnityEngine;

namespace SS
{
	[DisallowMultipleComponent]
	public class PauseController : MonoBehaviour
	{
		public void Pause()
		{
			if( PauseManager.isPaused )
			{
				return;
			}
			PauseManager.Pause();
		}

		public void Unpause()
		{
			if( !PauseManager.isPaused )
			{
				return;
			}
			PauseManager.Unpause();
		}
	}
}