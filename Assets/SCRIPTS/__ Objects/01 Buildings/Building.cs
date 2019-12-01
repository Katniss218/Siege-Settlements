using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Buildings
{
	public class Building : SSObjectSelectable, IHUDHolder, IDamageable, IUsableToggle, IFactionMember, IObstacle
	{
		// The amount of health that the building marked as being constructed is going to start with.
		public const float STARTING_HEALTH_PERCENT = 0.1f;


		public GameObject hud { get; set; }

		public Vector3[] placementNodes { get; set; }

		public Vector3? entrance { get; set; }

		public Dictionary<string, int> StartToEndConstructionCost { get; set; }

		public AudioClip buildSoundEffect { get; set; }
		

		public AudioClip deathSound { get; set; }

		public bool IsUsable()
		{
			// If not under construction/repair.
			if( this.GetComponent<ConstructionSite>() == null )
			{
				return this.damageable.healthPercent >= 0.5f;
			}
			// If under construction/repair.
			return false;
		}
		

		/// <summary>
		/// Checks if the building can be repaired (repair hasn't started already).
		/// </summary>
		public static bool IsRepairable( Damageable building )
		{
			// If the construction/repair is currently being done.
			if( building.GetComponent<ConstructionSite>() != null )
			{
				return false;
			}
			// If the construction/repair is NOT being done.
			return building.health < building.healthMax;
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

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Health: " + (int)this.damageable.health + "/" + (int)this.damageable.healthMax );
			SelectionPanel.instance.obj.RegisterElement( "building.health", healthUI.transform );

			if( this.factionMember.factionId == LevelDataManager.PLAYER_FAC )
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
					this.damageable.Die();
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