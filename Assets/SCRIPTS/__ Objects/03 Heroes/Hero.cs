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
	}
}