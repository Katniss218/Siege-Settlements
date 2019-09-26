using System;
using UnityEngine;

namespace SS.Modules
{
	public class MeleeModule : Module, ITargetFinder
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

		public DamageSource damageSource;
		public float attackCooldown;
		public AudioClip attackSoundEffect;

		private FactionMember factionMember;
		private float lastAttackTimestamp;

		public bool isReadyToAttack
		{
			get
			{
				return Time.time >= this.lastAttackTimestamp + this.attackCooldown;
			}
		}

		void Awake()
		{
			this.factionMember = this.GetComponent<FactionMember>();
		}

		private void Start()
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
					this.Attack( target );
					AudioManager.PlayNew( this.attackSoundEffect );
				}
			}
		}

		public void SetDefinition( MeleeModuleDefinition def )
		{
			this.canTarget = FactionMember.CanTargetCheck;
			this.searchRange = def.attackRange;
			
			DamageSource damageSource = new DamageSource( def.damageType, def.damage, def.armorPenetration );

			this.damageSource = damageSource;
			this.attackCooldown = def.attackCooldown;
			this.attackSoundEffect = def.attackSoundEffect.Item2;
		}

		/// <summary>
		/// Forces MeleeComponent to shoot at the target (assumes target != null).
		/// </summary>
		public void Attack( Damageable target )
		{
			target.TakeDamage( this.damageSource.damageType, this.damageSource.GetRandomizedDamage(), this.damageSource.armorPenetration );
			this.lastAttackTimestamp = Time.time;
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if( this.__target != null )
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine( this.transform.position, this.__target.transform.position );
				Gizmos.DrawSphere( this.__target.transform.position, 0.125f );
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere( this.transform.position, this.searchRange );
		}
#endif
	}
}