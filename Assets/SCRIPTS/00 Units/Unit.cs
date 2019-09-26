using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Units
{
	public class Unit : MonoBehaviour
	{
		public static bool IsValid( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.UNITS )
			{
				return false;
			}
			if( gameObject.GetComponent<Unit>() == null )
			{
				return false;
			}
			return true;
		}

		private static List<GameObject> _allUnits = new List<GameObject>();

		public static GameObject[] GetAllUnits()
		{
			return _allUnits.ToArray();
		}

		public Guid guid { get; set; }

		public string defId { get; set; }


		public string displayName { get; set; }


		void OnEnable()
		{
			_allUnits.Add( this.gameObject );
		}

		void OnDisable()
		{
			_allUnits.Remove( this.gameObject );
		}
	}
}