using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Extras
{
	public class Extra : SSObject
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