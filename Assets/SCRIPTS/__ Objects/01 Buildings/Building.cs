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

		private Selectable selectable = null;
		public Damageable damageable { get; set; }
		public FactionMember factionMember { get; set; }
		public NavMeshObstacle obstacle { get; private set; }
		new public BoxCollider collider { get; private set; }
		
		void Start()
		{
			this.selectable = this.GetComponent<Selectable>();
			this.damageable = this.GetComponent<Damageable>();
			this.factionMember = this.GetComponent<FactionMember>();
			this.obstacle = this.GetComponent<NavMeshObstacle>();
			this.collider = this.GetComponent<BoxCollider>();
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