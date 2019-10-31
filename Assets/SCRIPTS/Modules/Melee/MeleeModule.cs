using SS.Diplomacy;
using System;
using UnityEngine;

namespace SS.Modules
{
	public class MeleeModule : Module, ITargetFinder
	{
		private Damageable __target;


		public float searchRange { get; set; }
		
		public Func<FactionMember, FactionMember, bool> canTarget { get; set; }

		public Damageable target
		{
			get
			{
				if( this.CanTarget( this.__target ) )
				{
					this.__target = null;
				}
				return this.__target;
			}
		}

		public bool CanTarget( Damageable target )
		{
			if( target == null )
			{
				return false;
			}
		
			if( Vector3.Distance( target.transform.position, this.transform.position ) > this.searchRange )
			{
				return false;
			}

			FactionMember targetFactionMember = target.GetComponent<FactionMember>();
			if( !this.canTarget.Invoke( this.factionMember, targetFactionMember ) )
			{
				return false;
			}
			
			return true;
		}

		public Damageable TrySetTarget()
		{
			this.__target = this.FindTarget( this.searchRange );
			return this.__target;
		}

		public Damageable TrySetTarget( Damageable target )
		{
			if( this.CanTarget( target ) )
			{
				this.__target = target;
			}
			return this.__target;
		}
		
		private Damageable FindTarget( float searchRange )
		{
			Collider[] col = Physics.OverlapSphere( this.transform.position, searchRange, ObjectLayer.OBJECTS_MASK );
			if( col.Length == 0 )
			{
				return null;
			}
			
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
				if( !this.canTarget.Invoke( this.factionMember, targetFactionMember ) )
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
				// Get target, if current target is not targetable or no target is present - try to find a suitable one.
				if( !this.CanTarget( this.__target ) )
				{
					this.__target = this.TrySetTarget();
				}
				
				if( this.__target == null )
				{
					return;
				}
				
				this.Attack( this.__target );
				AudioManager.PlaySound( this.attackSoundEffect );
			}
		}

		public override ModuleData GetData()
		{
#warning TODO! - targets get saved.
			throw new NotImplementedException( "Can't get data of a melee module." );
		}

		public override void SetDefinition( ModuleDefinition _def )
		{
			if( !(_def is MeleeModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}

			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			MeleeModuleDefinition def = (MeleeModuleDefinition)_def;

			this.canTarget = FactionMember.CanTargetAnother;
			this.searchRange = def.attackRange;
			
			DamageSource damageSource = new DamageSource( def.damageType, def.damage, def.armorPenetration );

			this.damageSource = damageSource;
			this.attackCooldown = def.attackCooldown;
			this.attackSoundEffect = def.attackSoundEffect;
		}

		public override void SetData( ModuleData data )
		{
#warning TODO! - targets get saved.
			throw new NotImplementedException( "Can't assign data to a melee module." );
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