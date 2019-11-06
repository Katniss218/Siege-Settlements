using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Heroes
{
	public class Hero : SSObject, IHUDObject
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


		public GameObject hud { get; set; }

		public string displayTitle { get; set; }
		
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