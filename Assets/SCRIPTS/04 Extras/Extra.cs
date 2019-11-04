using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Extras
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
			_allExtras.Add( this );
		}

		void OnDisable()
		{
			_allExtras.Remove( this );
		}
	}
}