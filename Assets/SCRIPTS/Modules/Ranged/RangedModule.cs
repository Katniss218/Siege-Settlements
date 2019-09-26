using Katniss.Utils;
using SS.Content;
using SS.Levels.SaveStates;
using SS.Projectiles;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Modules
{
	public class RangedModule : Module
	{
		private Damageable __target;


		public float searchRange { get; set; }

		// Recalculate the target, when the target needs to be accessed.
		// Two types, melee and ranged. ranged takes into account the projectile trajectory, and doesn't target behind walls/etc.

		public Func<FactionMember, FactionMember, bool> canTarget { get; set; }

		public Damageable GetTarget()
		{
			// If the target is null, try to find new one.
			if( this.__target == null )
			{
				this.__target = this.FindTarget( this.searchRange );
			}
			// If it's not null, but can no longer be targeted, try to find new one.
			else if( Vector3.Distance( this.__target.transform.position, this.transform.position ) > this.searchRange )
			{
				this.__target = this.FindTarget( this.searchRange );
			}
			return this.__target;
		}

		private Damageable FindTarget( float searchRange )
		{
			Collider[] col = Physics.OverlapSphere( this.transform.position, searchRange );
			if( col.Length == 0 )
			{
				return null;
			}

			FactionMember selfFactionMember = this.GetComponent<FactionMember>();

			for( int i = 0; i < col.Length; i++ )
			{
				// If the overlapped object can't be damaged.
				Damageable potentialTarget = col[i].GetComponent<Damageable>();
				if( potentialTarget == null )
				{
					continue;
				}

				FactionMember targetFactionMember = col[i].GetComponent<FactionMember>();

				// Check if the overlapped object can be targeted by this finder.
				if( !this.canTarget.Invoke( selfFactionMember, targetFactionMember ) )
				{
					continue;
				}

				return potentialTarget;
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
			if( navmeshAgent == null )
			{
				targetVel = Vector3.zero;
			}
			else
			{
				targetVel = navmeshAgent.velocity;
			}
			if( BallisticSolver.Solve( this.transform.position, this.velocity, target.transform.position, targetVel, -Physics.gravity.y, out low, out high ) > 0 )
			{
				Vector3 pos;
				Vector3 vel = low;
				Matrix4x4 toWorld = this.transform.localToWorldMatrix;
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
			}
			this.lastAttackTimestamp = Time.time;
		}

		private void Shoot( Vector3 pos, Vector3 vel )
		{
			ProjectileData data = new ProjectileData();
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
			this.factionMember = this.GetComponent<FactionMember>();
		}

		void Start()
		{
			this.lastAttackTimestamp = UnityEngine.Random.Range( -this.attackCooldown, 0.0f );
		}

		void Update()
		{
			if( this.isReadyToAttack )
			{
				Damageable target = this.GetTarget();

				if( target != null )
				{
					if( target.transform.position == this.transform.position )
					{
						return;
					}
					this.Attack( target );
					AudioManager.PlayNew( this.attackSoundEffect );
				}
			}
		}

		public void SetDefinition( RangedModuleDefinition def )
		{
			this.canTarget = FactionMember.CanTargetCheck;
			this.searchRange = def.attackRange;


			DamageSource damageSource = new DamageSource( def.damageType, def.damage, def.armorPenetration );

			this.projectile = DefinitionManager.Get<ProjectileDefinition>( def.projectileId );
			this.damageSource = damageSource;
			this.projectileCount = def.projectileCount;
			this.attackCooldown = def.attackCooldown;
			this.velocity = def.velocity;
			this.localOffsetMin = def.localOffsetMin;
			this.localOffsetMax = def.localOffsetMax;
			this.attackSoundEffect = def.attackSoundEffect.Item2;
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color( 1.0f, 0.4f, 0.0f );
			Gizmos.DrawWireSphere( this.transform.position, BallisticSolver.GetMaxRange( this.velocity, -Physics.gravity.y, 0.0f ) );

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