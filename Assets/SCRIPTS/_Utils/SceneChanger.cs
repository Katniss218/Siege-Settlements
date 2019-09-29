using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Katniss.Utils
{
	public class SceneChanger : MonoBehaviour
	{
		public void SwitchScene( string sceneName )
		{
			SceneManager.LoadScene( sceneName, LoadSceneMode.Additive );
		}

		public void SwitchScene( int buildIndex )
		{
			SceneManager.LoadScene( buildIndex, LoadSceneMode.Additive );
		}
	}
}