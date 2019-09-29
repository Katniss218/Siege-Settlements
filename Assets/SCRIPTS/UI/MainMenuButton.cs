using SS.Levels;
using UnityEngine;

namespace SS.UI
{
	public class MainMenuButton : MonoBehaviour
	{
		public void _Trigger()
		{
			LevelManager.UnloadLevel();
		}
	}
}