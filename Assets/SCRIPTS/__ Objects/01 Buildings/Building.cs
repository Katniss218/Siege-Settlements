using SS.Diplomacy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Buildings
{
	public class Building : SSObject, IHUDHolder, IDamageable, IFactionMember, IObstacle
	{
		// The amount of health that the building marked as being constructed is going to start with.
		public const float STARTING_HEALTH_PERCENT = 0.1f;


		public GameObject hud { get; set; }

		public Vector3[] placementNodes { get; set; }

		public Vector3? entrance { get; set; }

		public Dictionary<string, int> StartToEndConstructionCost { get; set; }

		public AudioClip buildSoundEffect { get; set; }
		

		public AudioClip deathSound { get; set; }


		/// <summary>
		/// Checks if the building is in a 'usable' state (not under construction/repair and not below 50% health).
		/// </summary>
		public static bool IsUsable( Damageable building )
		{
			// If not under construction/repair.
			if( building.GetComponent<ConstructionSite>() == null )
			{
				return building.healthPercent >= 0.5f;
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
		
#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.white;

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			for( int i = 0; i < this.placementNodes.Length; i++ )
			{
				Gizmos.DrawSphere( toWorld.MultiplyVector( this.placementNodes[i] ) + this.transform.position, 0.05f );
			}
		}
#endif
	}
}