using UnityEngine;

namespace SS
{
	[RequireComponent( typeof( FactionMember ) )]
	[RequireComponent( typeof( DamageSource ) )]
	[RequireComponent( typeof( ITargetFinder ) )]
	public class MeleeModule : MonoBehaviour
	{
		public Damageable currentTarget;

		public DamageSource damageSource;
		public ITargetFinder targetFinder;

		public float attackRange;
		public float attackCooldown;

		public AudioClip attackSoundEffect;

		private float lastAttackTimestamp; // TODO ----- maybe separate something like "PeriodicalTriggerWithCondition" from this (lastattack timestamp, etc.).
		private FactionMember factionMember;

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
			if( damageSource == null )
			{
				Debug.LogError( "There's no damage source hooked up to this melee module." );
			}
			if( targetFinder == null )
			{
				Debug.LogError( "There's no target finder hooked up to this melee module." );
			}
			this.lastAttackTimestamp = Random.Range( -this.attackCooldown, 0.0f );
		}

		void Update()
		{
			if( this.isReadyToAttack )
			{
				this.currentTarget = this.targetFinder.FindTarget( this.attackRange );

				if( this.currentTarget != null )
				{
					this.Attack();
					AudioManager.PlayNew( this.attackSoundEffect, 1.0f, 1.0f );
				}
			}
		}

		

		/// <summary>
		/// Forces MeleeComponent to shoot at the target (assumes target != null).
		/// </summary>
		public void Attack()
		{
			this.currentTarget.TakeDamage( this.damageSource.damageType, this.damageSource.damage, this.damageSource.armorPenetration );
			this.lastAttackTimestamp = Time.time;
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if( this.currentTarget != null )
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine( this.transform.position, this.currentTarget.transform.position );
				Gizmos.DrawSphere( this.currentTarget.transform.position, 0.125f );
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere( this.transform.position, this.attackRange );
		}
#endif
	}
}