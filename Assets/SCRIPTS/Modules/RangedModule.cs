using Katniss.Utils;
using SS.Data;
using SS.Projectiles;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	[RequireComponent( typeof( ITargetFinder ) )]
	public class RangedModule : Module
	{
		public ProjectileDefinition projectile;
		public int projectileCount;

		public DamageSource damageSource;
		public ITargetFinder targetFinder;
		
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
			}
			this.lastAttackTimestamp = Time.time;
		}

		private void Shoot( Vector3 pos, Vector3 vel )
		{
			ProjectileCreator.Create( this.projectile, pos, vel, this.factionMember.factionId, this.damageSource.damageType, this.damageSource.damage, this.damageSource.armorPenetration, this.transform );
		}

		void Awake()
		{
			this.factionMember = this.GetComponent<FactionMember>();
		}

		void Start()
		{
			if( this.damageSource == null )
			{
				Debug.LogError( "There's no damage source hooked up to this ranged module." );
			}
			if( this.targetFinder == null )
			{
				Debug.LogError( "There's no target finder hooked up to this ranged module." );
			}
			this.lastAttackTimestamp = Random.Range( -this.attackCooldown, 0.0f );
		}

		void Update()
		{
			if( this.isReadyToAttack )
			{
				Damageable target = this.targetFinder.GetTarget();

				if( target != null )
				{
					if( target.transform.position == this.transform.position )
					{
						return;
					}
					this.Attack( target );
					AudioManager.PlayNew( this.attackSoundEffect, 1.0f, 1.0f );
				}
			}
		}
		
		public static void AddTo( GameObject obj, RangedModuleDefinition def )
		{
			TargetFinder finder = obj.AddComponent<TargetFinder>();

			finder.canTarget = FactionMember.CanTargetCheck;
			finder.searchRange = def.attackRange;


			DamageSource damageSource = new DamageSource( def.damageType, def.damage, def.armorPenetration );

			RangedModule ranged = obj.AddComponent<RangedModule>();
			ranged.projectile = DataManager.Get<ProjectileDefinition>( def.projectileId );
			ranged.damageSource = damageSource;
			ranged.targetFinder = finder;
			ranged.projectileCount = def.projectileCount;
			ranged.attackCooldown = def.attackCooldown;
			ranged.velocity = def.velocity;
			ranged.localOffsetMin = def.localOffsetMin;
			ranged.localOffsetMax = def.localOffsetMax;
			ranged.attackSoundEffect = def.attackSoundEffect.Item2;
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