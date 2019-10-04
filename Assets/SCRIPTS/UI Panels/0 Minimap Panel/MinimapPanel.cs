using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class MinimapPanel : MonoBehaviour
	{
		public static MinimapPanel instance { get; private set; }

		void Awake()
		{
			if( instance != null )
			{
				throw new System.Exception( "There is another minimap panel active" );
			}
			instance = this;
		}

		void Start()
		{

		}

		void Update()
		{

		}
	}
}