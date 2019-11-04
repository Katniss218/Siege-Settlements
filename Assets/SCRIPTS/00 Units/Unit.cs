using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Units
{
	public class Unit : SSObject
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

		private Guid? __guid = null;
		public Guid? guid
		{
			get
			{
				return this.__guid;
			}
			set
			{
				if( this.guid != null )
				{
					throw new Exception( "Tried to re-assign guid to '" + gameObject.name + "'. A guid is already assigned." );
				}
				this.__guid = value;
			}
		}

		private string __defId = null;
		public string defId
		{
			get
			{
				return this.__defId;
			}
			set
			{
				if( this.__defId != null )
				{
					throw new Exception( "Tried to assign definition to '" + gameObject.name + "' more than once." );
				}
				this.__defId = value;
			}
		}



		//public string displayName { get; set; }


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