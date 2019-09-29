using UnityEngine;
using UnityEngine.SceneManagement;

namespace SS
{
	/// <summary>
	/// Loads specified scene, when Start() is called.
	/// </summary>
	[DisallowMultipleComponent]
	public class DefaultSceneLoader : MonoBehaviour
	{
		[SerializeField] private string defaultSceneName = null;

		void Start()
		{
			if( string.IsNullOrEmpty( defaultSceneName ) )
			{
				throw new System.Exception( "The default scene hasn't been set." );
			}

			SceneManager.LoadScene( defaultSceneName, LoadSceneMode.Additive );
		}
	}
}