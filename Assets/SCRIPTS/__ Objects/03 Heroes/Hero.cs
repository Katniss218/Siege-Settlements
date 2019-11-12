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


		public bool hasBeenHiddenSinceLastDamage { get; set; }

		private Selectable selectable = null;
		private Damageable damageable = null;


		void Start()
		{
			this.selectable = this.GetComponent<Selectable>();
			this.damageable = this.GetComponent<Damageable>();
		}


		void Update()
		{
			if( !this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Main.isHudLocked )
			{
				return;
			}
			if( Selection.IsSelected( this.selectable ) )
			{
				return;
			}
			if( Time.time > this.damageable.lastDamageTakenTimestamp + SSObject.HUD_DAMAGE_DISPLAY_DURATION )
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					return;
				}
				this.hud.SetActive( false );
				this.hasBeenHiddenSinceLastDamage = false;
			}
		}

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