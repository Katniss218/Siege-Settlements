using SS.Content;
using SS.Levels.SaveStates;
using SS.ResourceSystem.Payment;
using SS.UI;
using SS.UI.HUDs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace SS.Objects.Buildings
{
	[UseHud(typeof(BuildingHUD), "hud")]
	public class Building : SSObjectDFC, ISSObjectUsableUnusable, IMouseOverHandlerListener
	{
		// The amount of health that the building marked as being constructed is going to start with.
		public const float STARTING_HEALTH_PERCENT = 0.1f;
		public const float UNUSABLE_THRESHOLD = 0.5f;

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
		public BuildingHUD hud { get; set; }
		public override HUDDFSC hudDFSC { get { return this.hud; } }


		public Vector3[] placementNodes { get; set; }
		
		public Dictionary<string, int> StartToEndConstructionCost { get; set; }

		public AudioClip buildSoundEffect { get; set; }

		internal ConstructionSite constructionSite { get; set; }
		public IPaymentReceiver paymentReceiver { get { return this.constructionSite; } }

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

		public UnityEvent onUsableStateChanged { get; private set; } = new UnityEvent();

		bool __isUsable = true;
		public bool isUsable
		{
			get
			{
				return this.__isUsable;
			}
			set
			{
				bool oldUsable = this.__isUsable;
				
				this.__isUsable = value;
				if( oldUsable != this.__isUsable )
				{
					this.onUsableStateChanged?.Invoke();
				}
			}
		}

		public bool IsUsable()
		{
			// If not under construction/repair.
			if( this.constructionSite == null )
			{
				return this.healthPercent >= UNUSABLE_THRESHOLD;
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
			this.hud.ConditionalShow();
		}

		public void OnMouseStayListener()
		{ }

		public void OnMouseExitListener()
		{
			this.hud.ConditionalHide();
		}

		void Update()
		{
			if( !this.isUsable )
			{
				this.hud.SnapToHolder();
			}
		}

		
		public void DisplayRepairButton()
		{
			if( Building.CanStartRepair( this ) )
			{
				// Display green repair icon if building can be repaired, red one if it needs to be repaired.
				Sprite repairIconSprite = null;
				if( this.isUsable )
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

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -25.0f ), new Vector2( 200.0f, 25.0f ), Vector2.up, Vector2.up, Vector2.up ), SSObjectDFC.GetHealthString( this.health, this.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "building.health", healthUI.transform );

			if( !this.IsDisplaySafe() )
			{
				return;
			}

			ConstructionSite constructionSite = this.GetComponent<ConstructionSite>();

			if( constructionSite != null )
			{
				constructionSite.Display();
			}
			if( !this.isUsable )
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