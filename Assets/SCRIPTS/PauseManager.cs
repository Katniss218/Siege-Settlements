using UnityEngine;

namespace SS
{
	public class PauseManager
	{
		public static bool isPaused { get; private set; }

		public static void Pause()
		{
#warning TODO - While paused, the player is still able to send inputs.
			isPaused = true;
			Time.timeScale = 0.0f;
		}

		public static void Unpause()
		{
			isPaused = false;
			Time.timeScale = 1.0f;
		}
	}
}