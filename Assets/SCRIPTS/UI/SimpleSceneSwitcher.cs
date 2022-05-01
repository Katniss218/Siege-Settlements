using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.SCRIPTS.UI
{
    public class SimpleSceneSwitcher : MonoBehaviour
    {
        public void LoadScene( string sceneName )
        {
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync( this.gameObject.scene );
            asyncOperation.completed += ( AsyncOperation oper ) =>
            {
                if( !SceneManager.GetSceneByName( sceneName ).isLoaded )
                {
                    SceneManager.LoadScene( sceneName, LoadSceneMode.Additive );
                }
            };
        }
    }
}