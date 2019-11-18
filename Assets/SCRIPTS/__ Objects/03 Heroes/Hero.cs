using SS.Diplomacy;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Heroes
{
	public class Hero : SSObject, IHUDHolder, IFactionMember, IDamageable
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
		public Damageable damageable { get; set; }
		public FactionMember factionMember { get; set; }


		void Start()
		{
			this.selectable = this.GetComponent<Selectable>();
			this.damageable = this.GetComponent<Damageable>();
			this.factionMember = this.GetComponent<FactionMember>();
		}


		void FixedUpdate()
		{
			if( hud.activeSelf )
			{
				hud.transform.position = Main.camera.WorldToScreenPoint( this.transform.position );
			}
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