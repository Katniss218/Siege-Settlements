using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Units
{
	public class Unit : SSObject, IHUDObject
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

		private static List<Unit> _allUnits = new List<Unit>();

		public static Unit[] GetAllUnits()
		{
			return _allUnits.ToArray();
		}


		public GameObject hud { get; set; }

		

		void OnEnable()
		{
			_allUnits.Add( this );
		}

		void OnDisable()
		{
			_allUnits.Remove( this );
		}
	}
}