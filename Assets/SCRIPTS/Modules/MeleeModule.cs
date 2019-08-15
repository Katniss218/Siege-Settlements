using UnityEngine;

namespace SS
{
	[RequireComponent( typeof( FactionMember ) )]
	[RequireComponent( typeof( DamageSource ) )]
	[RequireComponent( typeof( ITargetFinder ) )]
	public class MeleeModule : MonoBehaviour
	{
		public Damageable currentTarget;

		public DamageSource DamageSource;

		public float attackRange;
		public float attackCooldown;

		private float lastAttackTimestamp;
		private FactionMember factionMember;
		private ITargetFinder targetFinder;

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
			this.targetFinder = this.GetComponent<ITargetFinder>();
			this.lastAttackTimestamp = Random.Range( 0.0f, this.attackCooldown );
		}

		void Update()
		{
			if( this.isReadyToAttack )
			{
				this.currentTarget = this.targetFinder.FindTarget( this.attackRange );

				if( this.currentTarget != null )
				{
					this.Attack();
					AudioManager.PlayNew( Main.hitmelee, 1.0f, 1.0f );
				}
			}
		}

		

		/// <summary>
		/// Forces MeleeComponent to shoot at the target (assumes target != null).
		/// </summary>
		public void Attack()
		{
			this.currentTarget.TakeDamage( this.DamageSource.damageType, this.DamageSource.damage, this.DamageSource.armorPenetration );
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