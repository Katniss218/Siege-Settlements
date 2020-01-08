using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Buildings
{
	public class Building : SSObjectDFS, IHUDHolder, ISSObjectUsableUnusable, IMouseOverHandlerListener
	{
		// The amount of health that the building marked as being constructed is going to start with.
		public const float STARTING_HEALTH_PERCENT = 0.1f;

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

		/// <summary>
		/// Returns the hud that's attached to this object.
		/// </summary>
		public HUD hud { get; set; }

		public Vector3[] placementNodes { get; set; }
		
		public Dictionary<string, int> StartToEndConstructionCost { get; set; }

		public AudioClip buildSoundEffect { get; set; }

		internal ConstructionSite constructionSite { get; set; }

		public bool hasBeenHiddenSinceLastDamage { get; set; }

		private Vector3 __size;
		public Vector3 size
		{
			get
			{
				return this.__size;
			}
			set
			{
				this.__size = value;
				this.obstacle.size = value;
				this.obstacle.center = new Vector3( 0.0f, value.y / 2.0f, 0.0f );
				this.collider.size = value;
				this.collider.center = new Vector3( 0.0f, value.y / 2.0f, 0.0f );
			}
		}


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
		public static bool CanStartRepair( Building building )
		{
			// If the construction/repair is currently being done.
			if( building.GetComponent<ConstructionSite>() != null )
			{
				return false;
			}
			// If the construction/repair is NOT being done.
			return building.health < building.healthMax;
		}




#warning Only one inventory can be added to an object.
		// Inventory uses circular hud elements to display every slot in the inv.

#warning Only one resource deposit can be added to an object.
		// Inventory uses circular hud elements to display every slot in the inv.

#warning Only one interior can be added to an object.
		// Interior uses 2 rows of 6 icons to display slots.

			// worker slots in the top

			// generic & population slots at the bottom (in the same row).
		
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

		
		public void DisplayRepairButton()
		{
			if( Building.CanStartRepair( this ) )
			{
				// Display green repair icon if building can be repaired, red one if it needs to be repaired.
				Sprite repairIconSprite = null;
				if( this.IsUsable() )
				{
					repairIconSprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/repair_optional" );
				}
				else
				{
					repairIconSprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/repair" );
				}

				ActionPanel.instance.CreateButton( "building.ap.repair", repairIconSprite, "Repair", "Click to repair building...", () =>
				{
					ConstructionSiteData constructionSiteData = new ConstructionSiteData();

					ConstructionSite.BeginConstructionOrRepair( this, constructionSiteData );
					ActionPanel.instance.Clear( "building.ap.repair" );
				} );
			}
		}


		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -25.0f ), new Vector2( 200.0f, 25.0f ), Vector2.up, Vector2.up, Vector2.up ), SSObjectDFS.GetHealthDisplay( this.health, this.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "building.health", healthUI.transform );

			if( this.factionId == LevelDataManager.PLAYER_FAC )
			{
				ConstructionSite constructionSite = this.GetComponent<ConstructionSite>();

				if( constructionSite != null )
				{
					GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -50.0f ), new Vector2( 200.0f, 25.0f ), Vector2.up, Vector2.up, Vector2.up ), "Waiting for resources... " + constructionSite.GetStatusString() );
					SelectionPanel.instance.obj.RegisterElement( "building.construction_status", status.transform );
				}
				if( !this.IsUsable() )
				{
					GameObject unusableFlagUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -125.0f ), new Vector2( 200.0f, 25.0f ), Vector2.up, Vector2.up, Vector2.up ), "Not usable (under construction or <50% health)." );
					SelectionPanel.instance.obj.RegisterElement( "building.unusable_flag", unusableFlagUI.transform );
				}

				if( Building.CanStartRepair( this ) )
				{
					this.DisplayRepairButton();
				}

				ActionPanel.instance.CreateButton( "building.ap.demolish", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/demolish" ), "Demolish", "Click to demolish building...", () =>
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