using Katniss.Utils;
using SS.Projectiles;
using UnityEngine;

namespace SS
{
	// TODO! ----- some sort of base game object component core thing.
	[RequireComponent( typeof( IFactionMember ) )]
	public class RangedComponent : MonoBehaviour
	{
		public Damageable target { get; set; }

		public ProjectileDefinition projectile { get; set; }
		public int projectileCount { get; set; }

		public DamageType damageType { get; set; }
		public float damage { get; set; }
		public float armorPenetration { get; set; }

		public float attackRange { get; set; }
		public float attackCooldown { get; set; }
		public float velocity { get; set; }
		public Vector3 localOffsetMin { get; set; }
		public Vector3 localOffsetMax { get; set; }

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
		}

		void Start()
		{
			this.lastAttackTimestamp = Random.Range( -this.attackCooldown, 0.0f );
		}

		void Update()
		{
			if( this.isReadyToAttack )
			{
				this.FindTarget();

				if( this.target != null )
				{
					this.Attack();
				}
			}
		}

		/// <summary>
		/// Forces the RangedComponent to seek for targets.
		/// </summary>
		public void FindTarget()
		{
			Collider[] col = Physics.OverlapSphere( this.transform.position, this.attackRange );
			for( int i = 0; i < col.Length; i++ )
			{
				Damageable potentialTarget = col[i].GetComponent<Damageable>();
				if( potentialTarget == null )
				{
					continue;
				}
				if( potentialTarget is IFactionMember )
				{
					IFactionMember f = (IFactionMember)potentialTarget;
					if( f.factionId == this.factionMember.factionId )
					{
						//if( f.factionId == this.factionMember.factionId || Main.currentRelations[f.factionId, this.factionMember.factionId] != FactionRelation.Enemy )

						continue;
					}
				}
				this.target = potentialTarget;
				return;
			}
			this.target = null;
		}

		/// <summary>
		/// Forces RangedComponent to shoot at the target (assumes target != null).
		/// </summary>
		public void Attack()
		{
			Vector3 low, high;
			if( BallisticSolver.Solve( this.transform.position, this.velocity, this.target.transform.position, -Physics.gravity.y, out low, out high ) > 0 )
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

					this.Shoot( toWorld.MultiplyVector( pos ) + this.transform.position, vel );
				}
				this.lastAttackTimestamp = Time.time;
				AudioManager.PlayNew( Main.loose, 1.0f, 1.0f );
			}
		}

		private void Shoot( Vector3 pos, Vector3 vel )
		{
			Projectile.Create( this.projectile, pos, vel, this.factionMember.factionId, this.damage, this.transform );
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