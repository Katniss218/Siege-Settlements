using SS.Content;
using SS.Levels;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Buildings
{
	public class Building : SSObjectDFS, IHUDHolder, IUsableToggle, IDamageable, IFactionMember, IMouseOverHandlerListener
	{
		// The amount of health that the building marked as being constructed is going to start with.
		public const float STARTING_HEALTH_PERCENT = 0.1f;


		public HUD hud { get; set; }

		public Vector3[] placementNodes { get; set; }
		
		public Dictionary<string, int> StartToEndConstructionCost { get; set; }

		public AudioClip buildSoundEffect { get; set; }
		

		public AudioClip deathSound { get; set; }

		public bool IsUsable()
		{
			// If not under construction/repair.
			if( this.GetComponent<ConstructionSite>() == null )
			{
				return this.healthPercent >= 0.5f;
			}
			// If under construction/repair.
			return false;
		}
		

		/// <summary>
		/// Checks if the building can be repaired (repair hasn't started already).
		/// </summary>
		public static bool IsRepairable( Building building )
		{
			// If the construction/repair is currently being done.
			if( building.GetComponent<ConstructionSite>() != null )
			{
				return false;
			}
			// If the construction/repair is NOT being done.
			return building.health < building.healthMax.value;
		}
		
		public bool hasBeenHiddenSinceLastDamage { get; set; }
		
		private NavMeshObstacle __obstacle = null;
		public NavMeshObstacle obstacle
		{
			get
			{
				if( this.__obstacle == null )
				{
					this.__obstacle = this.GetComponent<NavMeshObstacle>();
				}
				return this.__obstacle;
			}
		}

		private BoxCollider __collider = null;
		new public BoxCollider collider
		{
			get
			{
				if( this.__collider == null )
				{
					this.__collider = this.GetComponent<BoxCollider>();
				}
				return this.__collider;
			}
		}

		public void OnMouseEnterListener()
		{
			if( Main.isHudForcedVisible ) { return; }

			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.isVisible = true;
		}

		public void OnMouseStayListener()
		{ }

		public void OnMouseExitListener()
		{
			if( Main.isHudForcedVisible ) { return; }

			if( this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.isVisible = false;
		}

		void Update()
		{
			if( this.hud.isVisible )
			{
				this.hud.transform.position = Main.camera.WorldToScreenPoint( this.transform.position );
			}

			if( !this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Main.isHudForcedVisible )
			{
				return;
			}
			if( Selection.IsSelected( this ) )
			{
				return;
			}
			if( Time.time > this.lastDamageTakenTimestamp + SSObject.HUD_DAMAGE_DISPLAY_DURATION )
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					return;
				}
				this.hud.isVisible = false;
				this.hasBeenHiddenSinceLastDamage = false;
			}
		}

		

		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Health: " + (int)this.health + "/" + (int)this.healthMax.value );
			SelectionPanel.instance.obj.RegisterElement( "building.health", healthUI.transform );

			if( this.factionId == LevelDataManager.PLAYER_FAC )
			{
				ConstructionSite constructionSite = this.GetComponent<ConstructionSite>();

				if( constructionSite != null )
				{
					GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources... " + constructionSite.GetStatusString() );
					SelectionPanel.instance.obj.RegisterElement( "building.construction_status", status.transform );
				}
				if( !this.IsUsable() )
				{
					GameObject unusableFlagUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "The building is not usable (under construction/repair or <50% health)." );
					SelectionPanel.instance.obj.RegisterElement( "building.unusable_flag", unusableFlagUI.transform );
				}
				ActionPanel.instance.CreateButton( "building.ap.demolish", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/demolish" ), "Demolish", "Press to demolish building.", () =>
				{
					this.Die();
				} );
			}
		}

		public override void OnHide()
		{

		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if( this.placementNodes != null )
			{
				Gizmos.color = Color.white;

				Matrix4x4 toWorld = this.transform.localToWorldMatrix;
				for( int i = 0; i < this.placementNodes.Length; i++ )
				{
					Gizmos.DrawSphere( toWorld.MultiplyVector( this.placementNodes[i] ) + this.transform.position, 0.05f );
				}
			}
		}
#endif
	}
}