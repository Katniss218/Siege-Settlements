using UnityEngine;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	[RequireComponent( typeof( ITargetFinder ) )]
	public class MeleeModule : Module
	{
		//public Damageable currentTarget;

		public DamageSource damageSource;
		public ITargetFinder targetFinder;

		public float attackRange;
		public float attackCooldown;

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

		void Awake()
		{
			this.factionMember = this.GetComponent<FactionMember>();
		}

		private void Start()
		{
			if( this.damageSource == null )
			{
				Debug.LogError( "There's no damage source hooked up to this melee module." );
			}
			if( this.targetFinder == null )
			{
				Debug.LogError( "There's no target finder hooked up to this melee module." );
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
					this.Attack( target );
					AudioManager.PlayNew( this.attackSoundEffect, 1.0f, 1.0f );
				}
			}
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

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere( this.transform.position, this.attackRange );
		}
#endif
	}
}