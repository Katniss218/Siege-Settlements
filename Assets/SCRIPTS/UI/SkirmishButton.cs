using SS.Levels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.UI
{
	public class SkirmishButton : MonoBehaviour
	{
		public void Click()
		{

			LevelManager.Load( Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Levels" + System.IO.Path.DirectorySeparatorChar + "Tutorial" );
		}
		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}