using UnityEngine;

namespace SS
{
	[RequireComponent( typeof( FactionMember ) )]
	public class MeleeModule : MonoBehaviour
	{
		public Damageable currentTarget;

		public DamageSource DamageSource;

		public float attackRange;
		public float attackCooldown;

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
			this.lastAttackTimestamp = Random.Range( 0.0f, this.attackCooldown );
		}

		void Update()
		{
			if( this.isReadyToAttack )
			{
				this.FindTarget();

				if( this.currentTarget != null )
				{
					this.Attack();
					AudioManager.PlayNew( Main.hitmelee, 1.0f, 1.0f );
				}
			}
		}

		/// <summary>
		/// Forces the MeleeComponent to seek for targets.
		/// </summary>
		public void FindTarget()
		{
			Collider[] col = Physics.OverlapSphere( this.transform.position, this.attackRange );
			for( int i = 0; i < col.Length; i++ )
			{
				Damageable d = col[i].GetComponent<Damageable>();
				if( d == null )
				{
					continue;
				}
				FactionMember f = d.GetComponent<FactionMember>();
				if( f != null )
				{
					if( f.factionId == this.factionMember.factionId )//|| Main.currentRelations[f.factionId, this.factionMember.factionId] != FactionRelation.Enemy )
					{
						continue;
					}
				}
				
				this.currentTarget = d;
				return;
			}
			this.currentTarget = null;
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