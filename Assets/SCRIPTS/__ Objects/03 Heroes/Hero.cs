using SS.Diplomacy;
using SS.UI;
using TMPro;
using UnityEngine;

namespace SS.Objects.Heroes
{
	public class Hero : SSObjectSelectable, IHUDHolder, IDamageable, IFactionMember
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
				base.displayName = value;
				this.hud.transform.Find( "Name" ).GetComponent<TextMeshProUGUI>().text = value;
				if( Selection.IsDisplayed( this ) )
				{
					SelectionPanel.instance.obj.displayNameText.text = value;
				}
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
				
				if( Selection.IsDisplayed( this ) )
				{
					Transform titleUI = SelectionPanel.instance.obj.GetElement( "hero.title" );
					if( titleUI != null )
					{
						UIUtils.EditText( titleUI.gameObject, value );
					}
				}
			}
		}


		public bool hasBeenHiddenSinceLastDamage { get; set; }
		
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
			if( Selection.IsSelected( this ) )
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

		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject titleUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), this.displayTitle );
			SelectionPanel.instance.obj.RegisterElement( "hero.title", titleUI.transform );

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Health: " + (int)this.damageable.health + "/" + (int)this.damageable.healthMax );
			SelectionPanel.instance.obj.RegisterElement( "hero.health", healthUI.transform );
		}
	}
}