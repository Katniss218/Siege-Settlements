using UnityEngine;

namespace SS
{
	[RequireComponent( typeof( IFactionMember ) )]
	public class MeleeComponent : MonoBehaviour
	{
		//public MeleeComponentDefinition definition { get; set; }
		public Damageable target { get; set; }

		public DamageType damageType { get; set; }
		public float damage { get; set; }
		public float armorPenetration { get; set; }

		public float attackRange { get; set; }
		public float attackCooldown { get; set; }

		private float lastAttackTimestamp;
		private IFactionMember factionMember;

		public bool isReadyToAttack
		{
			get
			{
				return Time.time >= this.lastAttackTimestamp + this.attackCooldown;
			}
		}

		void Awake()
		{
			this.factionMember = this.GetComponent<IFactionMember>();
			this.lastAttackTimestamp = Random.Range( 0.0f, this.attackCooldown );
		}

		void Update()
		{
			if( this.isReadyToAttack )
			{
				this.FindTarget();

				if( this.target != null )
				{
					this.Attack();
					AudioManager.PlayNew( Main.instance.hitmelee, 1.0f, 1.0f );
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
				if( d is IFactionMember )
				{
					IFactionMember f = (IFactionMember)d;
					if( f.factionId == this.factionMember.factionId )//|| Main.currentRelations[f.factionId, this.factionMember.factionId] != FactionRelation.Enemy )
					{
						continue;
					}
				}
				
				this.target = d;
				return;
			}
			this.target = null;
		}

		/// <summary>
		/// Forces MeleeComponent to shoot at the target (assumes target != null).
		/// </summary>
		public void Attack()
		{
			this.target.TakeDamage( this.damageType, this.damage, this.armorPenetration );
			this.lastAttackTimestamp = Time.time;
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
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere( this.transform.position, this.attackRange );
		}
#endif
	}
}