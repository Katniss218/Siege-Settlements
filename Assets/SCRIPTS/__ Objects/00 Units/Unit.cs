using SS.Diplomacy;
using UnityEngine;

namespace SS.Objects.Units
{
	public class Unit : SSObject, IHUDHolder, IFactionMember, IDamageable
	{
		public GameObject hud { get; set; }

		public bool hasBeenHiddenSinceLastDamage { get; set; }

		private Selectable selectable = null;
		public Damageable damageable { get; set; }
		public FactionMember factionMember { get; set; }


		void Start()
		{
			this.selectable = this.GetComponent<Selectable>();
			if( this.selectable == null )
			{
				throw new System.Exception( "Invalid Unit." );
			}
			this.damageable = this.GetComponent<Damageable>();
			if( this.damageable == null )
			{
				throw new System.Exception( "Invalid Unit." );
			}
			this.factionMember = this.GetComponent<FactionMember>();
			if( this.factionMember == null )
			{
				throw new System.Exception( "Invalid Unit." );
			}
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
	}
}