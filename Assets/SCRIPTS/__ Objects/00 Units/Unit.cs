using SS.Diplomacy;
using SS.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS.Objects.Units
{
	public class Unit : SSObjectSelectable, IHUDHolder, IDamageable, IFactionMember, IPointerEnterHandler
	{
		public GameObject hud { get; set; }

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

		public void OnPointerEnter( PointerEventData eventData )
		{
			Debug.Log( this.definitionId );
		}

		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Health: " + (int)this.damageable.health + "/" + (int)this.damageable.healthMax );
			SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );
		}
	}
}