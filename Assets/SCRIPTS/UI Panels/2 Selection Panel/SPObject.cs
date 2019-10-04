using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class SPObject : MonoBehaviour
	{
		void Start()
		{

		}

		void Update()
		{

		}

		/// <summary>
		/// Clears every UI element that belongs to highlighted objects.
		/// </summary>
		public void Clear()
		{
			for( int i = 0; i < this.transform.childCount; i++ )
			{
				Destroy( this.transform.GetChild( i ).gameObject );
			}
		}
	}
}