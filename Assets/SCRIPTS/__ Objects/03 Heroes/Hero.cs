using SS.Diplomacy;
using TMPro;
using UnityEngine;

namespace SS.Objects.Heroes
{
	public class Hero : SSObject, IHUDHolder, IDamageable, IFactionMember
	{
		public GameObject hud { get; set; }

		private string __displayTitle = "<missing>";

		/// <summary>
		/// Gets or sets the display name of this Hero.
		/// </summary>
		public override string displayName
		{
			get => base.displayName;
			set
			{
				this.hud.transform.Find( "Name" ).GetComponent<TextMeshProUGUI>().text = value;
				base.displayName = value;
			}
		}

		/// <summary>
		/// Gets or sets the display title of this Hero.
		/// </summary>
		public string displayTitle
		{
			get
			{
				return this.__displayTitle;
			}
			set
			{
				this.__displayTitle = value;
				this.hud.transform.Find( "Title" ).GetComponent<TextMeshProUGUI>().text = value;
			}
		}


		public bool hasBeenHiddenSinceLastDamage { get; set; }

		private Selectable __selectable = null;
		public Selectable selectable
		{
			get
			{
				if( this.__selectable == null )
				{
					this.__selectable = this.GetComponent<Selectable>();
				}
				return this.__selectable;
			}
		}

		private Damageable __damageable = null;
		public Damageable damageable
		{
			get
			{
				if( this.__damageable == null )
				{
					this.__damageable = this.GetComponent<Damageable>();
				}
				return this.__damageable;
			}
		}

		private FactionMember __factionMember = null;
		public FactionMember factionMember
		{
			get
			{
				if( this.__factionMember == null )
				{
					this.__factionMember = this.GetComponent<FactionMember>();
				}
				return this.__factionMember;
			}
		}

		
		void Update()
		{
			if( hud.activeSelf )
			{
				hud.transform.position = Main.camera.WorldToScreenPoint( this.transform.position );
			}

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