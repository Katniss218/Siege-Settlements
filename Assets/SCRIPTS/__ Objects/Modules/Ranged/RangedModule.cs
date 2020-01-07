using Katniss.Utils;
using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using SS.Objects.Projectiles;
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using SS.Objects.Units;

namespace SS.Objects.Modules
{
	public class RangedModule : SSModule, IAttackModule
	{
		public const string KFF_TYPEID = "ranged";

		public float attackRange { get; set; }

		// it's the target finder.
		
		private SSObjectDFS __target;
		public SSObjectDFS target
		{
			get
			{
				return this.__target;
			}
			set
			{
				this.__target = value;
				if( value == null )
				{
					for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
					{
						this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
					}
				}
			}
		}

		public ProjectileDefinition projectile;
		public int projectileCount;
		public int? projectileCountOverride;
		
		public float damage;
		public float armorPenetration;
		public DamageType damageType;

		public float attackCooldown;
		public float velocity;

		public bool useHitboxOffset;
		public Vector3 localOffsetMin;
		public Vector3 localOffsetMax;

		public AudioClip attackSoundEffect;

		private float lastAttackTimestamp;
		private SSObjectDFS __factionMemberSelf = null;
		private SSObjectDFS factionMemberSelf
		{
			get
			{
				if( __factionMemberSelf == null )
				{
					__factionMemberSelf = this.ssObject as SSObjectDFS;
				}
				return __factionMemberSelf;
			}
		}

		public bool isReadyToAttack
		{
			get
			{
				return Time.time >= (this.lastAttackTimestamp + this.attackCooldown);
			}
		}

		private bool isReady2 = false;

		public SubObject[] traversibleSubObjects { get; set; }


		private Tuple<Vector3, Vector3> GetShootingOffsets()
		{
			if( this.useHitboxOffset )
			{
				if( this.ssObject is Unit )
				{
					BoxCollider col = ((Unit)this.ssObject).collider;
					
					Vector3 min = col.center - (col.size / 2);
					min.y = col.center.y + (col.size.y / 2);

					Vector3 max = col.center + (col.size / 2);
					max.y = col.center.y + (col.size.y / 2);

					return new Tuple<Vector3, Vector3>( min, max );
				}
			}
			return new Tuple<Vector3, Vector3>( this.localOffsetMin, this.localOffsetMax );
		}

		public void FindTargetClosest()
		{
			this.target = Targeter.FindTargetClosest( this.transform.position, this.attackRange, this.factionMemberSelf, true );
		}

		public void TrySetTarget( SSObjectDFS target )
		{
			this.target = Targeter.TrySetTarget( this.transform.position, this.attackRange, this.factionMemberSelf, target, true );
		}

		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		void Start()
		{
			this.lastAttackTimestamp = Random.Range( -this.attackCooldown, 0.0f ) + Time.time;
		}

		void Update()
		{
			// If it's not usable - return, don't attack.
			if( this.ssObject is IUsableSSObject && !((IUsableSSObject)this.ssObject).IsUsable() )
			{
				return;
			}

			if( this.ssObject is Unit && ((Unit)this.ssObject).isInsideHidden )
			{
				return;
			}

			if( this.target != null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.rotation = Quaternion.LookRotation( (this.target.transform.position - this.traversibleSubObjects[i].transform.position).normalized, this.transform.up );
				}

				if( this.isReady2 )
				{
					if( this.target.transform.position == this.transform.position )
					{
						return;
					}

					this.Attack( this.target );
				}
			}


			if( this.isReadyToAttack )
			{
				this.isReady2 = true;
			}
		}


		/// <summary>
		/// Forces RangedComponent to shoot at the target (assumes target != null).
		/// </summary>
		public void Attack( SSObject target )
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
				Tuple<Vector3, Vector3> offsets = this.GetShootingOffsets();
				Vector3 pos = Vector3.zero;
				Vector3 vel = low;
				int count = projectileCountOverride == null ? projectileCount : projectileCountOverride.Value;
				for( int i = 0; i < count; i++ )
				{

					pos = new Vector3(
						Random.Range( offsets.Item1.x, offsets.Item2.x ),
						Random.Range( offsets.Item1.y, offsets.Item2.y ),
						Random.Range( offsets.Item1.z, offsets.Item2.z )
					);

					Vector3 ranVel = vel;
					ranVel.x *= Random.Range( 0.9f, 1.1f );
					ranVel.y *= Random.Range( 0.95f, 1.05f );
					ranVel.z *= Random.Range( 0.9f, 1.1f );
					this.Shoot( toWorld.MultiplyVector( pos ) + this.transform.position, ranVel );
				}
				AudioManager.PlaySound( this.attackSoundEffect, this.transform.position );
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
			data.ownerFactionIdCache = (this.ssObject as SSObjectDFS).factionId;
			data.damageTypeOverride = this.damageType;
			data.damageOverride = this.damage;
			data.armorPenetrationOverride = this.armorPenetration;
			data.owner = new Tuple<Guid, Guid>(
				this.ssObject.guid,
				this.moduleId
				);

			GameObject projectile = ProjectileCreator.Create( this.projectile, data.guid );
			ProjectileCreator.SetData( projectile, data );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override ModuleData GetData()
		{
			RangedModuleData data = new RangedModuleData();

			if( this.target != null )
			{
				data.targetGuid = this.target.GetComponent<SSObject>().guid;
			}
			if( this.projectileCountOverride != null )
			{
				data.projectileCountOverride = this.projectileCountOverride;
			}
			return data;
		}

		public override void SetData( ModuleData _data )
		{
			RangedModuleData data = ValidateDataType<RangedModuleData>( _data );
			
			if( data.targetGuid != null )
			{
				this.target = (SSObject.Find( data.targetGuid.Value ) as SSObjectDFS);
			}
			if( data.projectileCountOverride != null )
			{
				this.projectileCountOverride = data.projectileCountOverride;
			}
		}
		
#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if( this.target != null )
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine( this.transform.position, this.target.transform.position );
				Gizmos.DrawSphere( this.target.transform.position, 0.125f );
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
			Tuple<Vector3, Vector3> offsets = this.GetShootingOffsets();
			for( int i = 0; i < 100; i++ )
			{
				pos = new Vector3(
					Random.Range( offsets.Item1.x, offsets.Item2.x ),
					Random.Range( offsets.Item1.y, offsets.Item2.y ),
					Random.Range( offsets.Item1.z, offsets.Item2.z )
				);
				Gizmos.DrawSphere( toWorld.MultiplyVector( pos ) + this.transform.position, 0.05f );
			}
		}
#endif
	}
}