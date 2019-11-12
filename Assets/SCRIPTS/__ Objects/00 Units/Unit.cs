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
			_allUnits.Add( this );
		}

		void OnDisable()
		{
			_allUnits.Remove( this );
		}
	}
}