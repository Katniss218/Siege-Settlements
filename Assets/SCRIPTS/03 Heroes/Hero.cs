using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Heroes
{
	public class Hero : SSObject
	{
		public static bool IsValid( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.HEROES )
			{
				return false;
			}
			if( gameObject.GetComponent<Hero>() == null )
			{
				return false;
			}
			return true;
		}

		private static List<Hero> _allHeroes = new List<Hero>();

		public static Hero[] GetAllHeroes()
		{
			return _allHeroes.ToArray();
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
		public string displayTitle { get; set; }

		public GameObject hudGameObject { get; set; }

		void OnEnable()
		{
			_allHeroes.Add( this );
		}

		void OnDisable()
		{
			_allHeroes.Remove( this );
		}
	}
}