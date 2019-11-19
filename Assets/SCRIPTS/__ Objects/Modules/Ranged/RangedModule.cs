using Katniss.Utils;
using SS.Objects.Buildings;
using SS.Content;
using SS.Diplomacy;
using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using SS.Objects.Projectiles;
using System;
using UnityEngine;
using UnityEngine.AI;
using SS.Objects;

namespace SS.Modules
{
	public class RangedModule : SSModule, ITargetFinder
	{
		private Damageable __target;


		public float searchRange { get; set; }

		// Recalculate the target, when the target needs to be accessed.
		// Two types, melee and ranged. ranged takes into account the projectile trajectory, and doesn't target behind walls/etc.
		
		public Damageable target
		{
			get
			{
				if( this.CanTarget( this.__target ) )
				{
					this.__target = null;
				}
				if( this.__target == null )
				{
					for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
					{
						this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
					}
				}
				return this.__target;
			}
		}

		public bool CanTarget( Damageable target )
		{
			if( target == null )
			{
				return false;
			}

			if( Vector3.Distance( target.transform.position, this.transform.position ) > this.searchRange )
			{
				return false;
			}

			if( !this.factionMember.CanTargetAnother( target.GetComponent<FactionMember>() ) )
			{
				return false;
			}

			return true;
		}

		public Damageable TrySetTarget()
		{
			this.__target = this.FindTarget( this.searchRange );
			if( this.__target == null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
				}
			}
			return this.__target;
		}

		public Damageable TrySetTarget( Damageable target )
		{
			if( this.CanTarget( target ) )
			{
				this.__target = target;
			}
			if( this.__target == null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
				}
			}
			return this.__target;
		}
		
		private Damageable FindTarget( float searchRange )
		{
			Collider[] col = Physics.OverlapSphere( this.transform.position, searchRange, ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK );
			
			if( col.Length == 0 )
			{
				return null;
			}

			Vector3 thisPosition = this.transform.position;
			for( int i = 0; i < col.Length; i++ )
			{
				SSObject ssObject = col[i].GetComponent<SSObject>();

				// Check if the overlapped object can be targeted by this finder.
				if( !this.factionMember.CanTargetAnother( (ssObject as IFactionMember).factionMember ) )
				{
					continue;
				}

				// Disregard potential targets, if the overlap is present, but the center is outside.
				if( Main.IsInRange( col[i].transform.position, thisPosition, searchRange ) )
				{
					return (ssObject as IDamageable).damageable;
				}
			}

			return null;
		}

		// it's the target finder.

		public ProjectileDefinition projectile;
		public int projectileCount;

		public DamageSource damageSource;
		
		public float attackCooldown;
		public float velocity;
		public Vector3 localOffsetMin;
		public Vector3 localOffsetMax;

		public AudioClip attackSoundEffect;

		private float lastAttackTimestamp;
		private FactionMember factionMember;

		private bool isBuilding;
		private Damageable damageableSelf;

		private SubObject[] traversibleSubObjects { get; set; }

		public bool isReadyToAttack
		{
			get
			{
				return Time.time >= this.lastAttackTimestamp + this.attackCooldown;
			}
		}

		/// <summary>
		/// Forces RangedComponent to shoot at the target (assumes target != null).
		/// </summary>
		public void Attack( Damageable target )
		{
			Vector3 low, high;
			Vector3 targetVel;
			NavMeshAgent navmeshAgent = target.GetComponent<NavMeshAgent>();
			BoxCollider collidertarget = target.GetComponent<BoxCollider>();

			Matrix4x4 enemyToWorld = target.transform.localToWorldMatrix;

			Vector3 enemyCenterWorld = target.transform.position;
			if( collidertarget != null )
			{
				enemyCenterWorld = enemyToWorld.MultiplyVector( new Vector3( 0.0f, collidertarget.size.y / 2.0f, 0.0f ) ) + target.transform.position;
			}
			if( navmeshAgent == null )
			{
				targetVel = Vector3.zero;
			}
			else
			{
				targetVel = navmeshAgent.velocity;
			}
			Matrix4x4 toWorld = this.transform.localToWorldMatrix;

			Vector3 boxCenterGlobal = toWorld.MultiplyVector( (this.localOffsetMin + this.localOffsetMax) / 2 ) + this.transform.position;

			if( BallisticSolver.Solve( boxCenterGlobal, this.velocity, enemyCenterWorld, targetVel, -Physics.gravity.y, out low, out high ) > 0 )
			{
				Vector3 pos;
				Vector3 vel = low;
				for( int i = 0; i < this.projectileCount; i++ )
				{
					pos = new Vector3(
						UnityEngine.Random.Range( this.localOffsetMin.x, this.localOffsetMax.x ),
						UnityEngine.Random.Range( this.localOffsetMin.y, this.localOffsetMax.y ),
						UnityEngine.Random.Range( this.localOffsetMin.z, this.localOffsetMax.z )
					);

					Vector3 ranVel = vel;
					ranVel.x *= UnityEngine.Random.Range( 0.9f, 1.1f );
					ranVel.y *= UnityEngine.Random.Range( 0.95f, 1.05f );
					ranVel.z *= UnityEngine.Random.Range( 0.9f, 1.1f );
					this.Shoot( toWorld.MultiplyVector( pos ) + this.transform.position, ranVel );
				}
				AudioManager.PlaySound( this.attackSoundEffect );
			}
			this.lastAttackTimestamp = Time.time;
		}

		private void Shoot( Vector3 pos, Vector3 vel )
		{
			ProjectileData data = new ProjectileData();
			data.guid = Guid.NewGuid();
			data.position = pos;
			data.velocity = vel;
			data.factionId = this.factionMember.factionId;
			data.damageTypeOverride = this.damageSource.damageType;
			data.damageOverride = this.damageSource.damage;
			data.armorPenetrationOverride = this.damageSource.armorPenetration;

			ProjectileCreator.Create( this.projectile, data );
		}

		void Awake()
		{
		}

		void Start()
		{
			this.lastAttackTimestamp = UnityEngine.Random.Range( -this.attackCooldown, 0.0f );
			this.isBuilding = (this.ssObject is Building);
			this.factionMember = this.GetComponent<FactionMember>();
			this.damageableSelf = this.GetComponent<Damageable>();
		}

		void Update()
		{
			// If it's a building and it's not usable - return, don't attack.
			if( this.isBuilding && !Building.IsUsable( this.damageableSelf ) )
			{
				return;
			}
			
			if( this.__target != null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.rotation = Quaternion.LookRotation( (this.__target.transform.position - this.traversibleSubObjects[i].transform.position).normalized, this.transform.up );
				}
			}

			if( this.isReadyToAttack )
			{
				// Get target, if current target is not targetable or no target is present - try to find a suitable one.
				
				if( !this.CanTarget( this.__target ) )
				{
					this.__target = this.TrySetTarget();
				}

				if( this.__target == null )
				{
					return;
				}

				if( this.__target.transform.position == this.transform.position )
				{
					return;
				}

				this.Attack( this.__target );
			}
		}

		public override ModuleData GetData()
		{
			RangedModuleData data = new RangedModuleData();

			if( this.target != null )
			{
				data.targetGuid = this.target.GetComponent<SSObject>().guid.Value;
			}
			return data;
		}

		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is RangedModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is RangedModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			RangedModuleDefinition def = (RangedModuleDefinition)_def;
			RangedModuleData data = (RangedModuleData)_data;
			
			this.searchRange = def.attackRange;


			DamageSource damageSource = new DamageSource( def.damageType, def.damage, def.armorPenetration );

			this.projectile = DefinitionManager.GetProjectile( def.projectileId );
			this.damageSource = damageSource;
			this.projectileCount = def.projectileCount;
			this.attackCooldown = def.attackCooldown;
			this.velocity = def.velocity;
			this.localOffsetMin = def.localOffsetMin;
			this.localOffsetMax = def.localOffsetMax;
			this.attackSoundEffect = def.attackSoundEffect;

			this.traversibleSubObjects = new SubObject[def.traversibleSubObjects.Length];
			for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
			{
				SubObject trav = this.ssObject.GetSubObject( def.traversibleSubObjects[i] );

				if( trav == null )
				{
					throw new Exception( "Can't find Sub-Object with Id of '" + def.traversibleSubObjects[i].ToString( "D" ) + "'." );
				}

				this.traversibleSubObjects[i] = trav;
			}

			if( data.targetGuid != null )
			{
				this.TrySetTarget( Main.GetSSObject( data.targetGuid.Value ).GetComponent<Damageable>() );
			}
		}
		
#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if( this.__target != null )
			{
				Gizmos.color = new Color( 0.0f, 0.0f, 1.0f );
				Gizmos.DrawSphere( this.__target.transform.position, 0.125f );
				Gizmos.DrawLine( this.transform.position, this.__target.transform.position );
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color( 1.0f, 0.4f, 0.0f );
			Gizmos.DrawWireSphere( this.transform.position, BallisticSolver.GetMaxRange( this.velocity, -Physics.gravity.y, 0.0f ) );

			Gizmos.color = new Color( 1.0f, 1.0f, 0.0f );
			Gizmos.DrawWireSphere( this.transform.position, this.searchRange );

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			Vector3 pos;
			Gizmos.color = Color.red;
			for( int i = 0; i < 100; i++ )
			{
				pos = new Vector3(
					UnityEngine.Random.Range( this.localOffsetMin.x, this.localOffsetMax.x ),
					UnityEngine.Random.Range( this.localOffsetMin.y, this.localOffsetMax.y ),
					UnityEngine.Random.Range( this.localOffsetMin.z, this.localOffsetMax.z )
				);
				Gizmos.DrawSphere( toWorld.MultiplyVector( pos ) + this.transform.position, 0.05f );
			}
		}
#endif
	}
}