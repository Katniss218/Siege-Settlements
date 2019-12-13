using Katniss.Utils;
using SS.Content;
using SS.Diplomacy;
using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using SS.Objects.Projectiles;
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SS.Objects.Modules
{
	public class RangedModule : SSModule, IAttackModule
	{
		public const string KFF_TYPEID = "ranged";

		public float attackRange { get; set; }

		// it's the target finder.

		public Targeter targeter { get; private set; }


		public ProjectileDefinition projectile;
		public int projectileCount;
		public DamageSource damageSource;
		public float attackCooldown;
		public float velocity;
		public Vector3 localOffsetMin;
		public Vector3 localOffsetMax;
		public AudioClip attackSoundEffect;

		private float lastAttackTimestamp;
		
		public bool isReadyToAttack
		{
			get
			{
				return Time.time >= (this.lastAttackTimestamp + this.attackCooldown);
			}
		}

		private bool isReady2 = false;

		private SubObject[] traversibleSubObjects { get; set; }


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

		void Awake()
		{
			this.targeter = new Targeter( ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK, this.GetComponent<FactionMember>() );

			this.targeter.onTargetReset += () =>
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
				}
			};
		}

		void Start()
		{
			this.lastAttackTimestamp = Random.Range( -this.attackCooldown, 0.0f ) + Time.time;
		}

		void Update()
		{
			// If it's not usable - return, don't attack.
			if( this.ssObject is IUsableToggle && !(this.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}
			
			if( this.targeter.target != null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.rotation = Quaternion.LookRotation( (this.targeter.target.transform.position - this.traversibleSubObjects[i].transform.position).normalized, this.transform.up );
				}
			}

			if( this.isReady2 )
			{
				if( this.targeter.target == null )
				{
					return;
				}

				if( this.targeter.target.transform.position == this.transform.position )
				{
					return;
				}

				this.Attack( this.targeter.target );
			}

			if( this.isReadyToAttack )
			{
				this.isReady2 = true;
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
						Random.Range( this.localOffsetMin.x, this.localOffsetMax.x ),
						Random.Range( this.localOffsetMin.y, this.localOffsetMax.y ),
						Random.Range( this.localOffsetMin.z, this.localOffsetMax.z )
					);

					Vector3 ranVel = vel;
					ranVel.x *= Random.Range( 0.9f, 1.1f );
					ranVel.y *= Random.Range( 0.95f, 1.05f );
					ranVel.z *= Random.Range( 0.9f, 1.1f );
					this.Shoot( toWorld.MultiplyVector( pos ) + this.transform.position, ranVel );
				}
				AudioManager.PlaySound( this.attackSoundEffect );
			}
			this.lastAttackTimestamp = Time.time;
			this.isReady2 = false;
		}

		private void Shoot( Vector3 pos, Vector3 vel )
		{
			ProjectileData data = new ProjectileData();
			data.guid = Guid.NewGuid();
			data.position = pos;
			data.velocity = vel;
			data.ownerFactionIdCache = this.targeter.factionMember.factionId;
			data.damageTypeOverride = this.damageSource.damageType;
			data.damageOverride = this.damageSource.damage;
			data.armorPenetrationOverride = this.damageSource.armorPenetration;
			data.owner = new Tuple<Guid, Guid>(
				this.ssObject.guid,
				this.moduleId
				);

			ProjectileCreator.Create( this.projectile, data );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override ModuleData GetData()
		{
			RangedModuleData data = new RangedModuleData();

			if( this.targeter.target != null )
			{
				data.targetGuid = this.targeter.target.GetComponent<SSObject>().guid;
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
			
			this.attackRange = def.attackRange;


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
				this.traversibleSubObjects[i] = trav ?? throw new Exception( "Can't find Sub-Object with Id of '" + def.traversibleSubObjects[i].ToString( "D" ) + "'." );
			}
			
			if( data.targetGuid != null )
			{
				this.targeter.target = (SSObject.Find( data.targetGuid.Value ) as IDamageable)?.damageable;
			}
		}
		
#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if( this.targeter.target != null )
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine( this.transform.position, this.targeter.target.transform.position );
				Gizmos.DrawSphere( this.targeter.target.transform.position, 0.125f );
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color( 1.0f, 0.4f, 0.0f );
			Gizmos.DrawWireSphere( this.transform.position, BallisticSolver.GetMaxRange( this.velocity, -Physics.gravity.y, 0.0f ) );

			Gizmos.color = new Color( 1.0f, 1.0f, 0.0f );
			Gizmos.DrawWireSphere( this.transform.position, this.attackRange );

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			Vector3 pos;
			Gizmos.color = Color.red;
			for( int i = 0; i < 100; i++ )
			{
				pos = new Vector3(
					Random.Range( this.localOffsetMin.x, this.localOffsetMax.x ),
					Random.Range( this.localOffsetMin.y, this.localOffsetMax.y ),
					Random.Range( this.localOffsetMin.z, this.localOffsetMax.z )
				);
				Gizmos.DrawSphere( toWorld.MultiplyVector( pos ) + this.transform.position, 0.05f );
			}
		}
#endif
	}
}