using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Extras
{
	public class Extra : MonoBehaviour
	{
		public static bool IsValid( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.EXTRAS )
			{
				return false;
			}
			if( gameObject.GetComponent<Extra>() == null )
			{
				return false;
			}
			return true;
		}

		private static List<Extra> _allExtras = new List<Extra>();

		public static Extra[] GetAllExtras()
		{
			return _allExtras.ToArray();
		}

		public Guid guid { get; set; }

		public string defId { get; set; }


		void OnEnable()
		{
			_allExtras.Add( this );
		}

		void OnDisable()
		{
			_allExtras.Remove( this );
		}
	}
}