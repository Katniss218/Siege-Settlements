using SS.Diplomacy;
using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using System;
using UnityEngine;
using SS.Objects;

namespace SS.Modules
{
	public class MeleeModule : SSModule, ITargetFinder
	{
		private Damageable __target;


		public float searchRange { get; set; }
		
		public Damageable target
		{
			get
			{
				if( this.CanTarget( this.__target ) )
				{
					this.__target = null;
				}
				if( this.__target == null )
				{
					for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
					{
						this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
					}
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
			
			if( !this.factionMember.CanTargetAnother( target.GetComponent<FactionMember>() ) )
			{
				return false;
			}
			
			return true;
		}

		public Damageable TrySetTarget()
		{
			this.__target = this.FindTarget( this.searchRange );
			if( this.__target == null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
				}
			}
			return this.__target;
		}

		public Damageable TrySetTarget( Damageable target )
		{
			if( this.CanTarget( target ) )
			{
				this.__target = target;
			}
			if( this.__target == null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
				}
			}
			return this.__target;
		}
		
		private Damageable FindTarget( float searchRange )
		{
			Collider[] col = Physics.OverlapSphere( this.transform.position, searchRange, ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK );
			if( col.Length == 0 )
			{
				return null;
			}

			Vector3 thisPosition = this.transform.position;
			for( int i = 0; i < col.Length; i++ )
			{
				SSObject ssObject = col[i].GetComponent<SSObject>();

				// Check if the overlapped object can be targeted by this finder.
				if( !this.factionMember.CanTargetAnother( (ssObject as IFactionMember).factionMember ) )
				{
					continue;
				}
				
				return (ssObject as IDamageable).damageable;
			}
			return null;
		}

		// it's the target finder.

		public DamageSource damageSource;
		public float attackCooldown;
		public AudioClip attackSoundEffect;

		private FactionMember factionMember;
		private float lastAttackTimestamp;
		
		private SubObject[] traversibleSubObjects { get; set; }

		public bool isReadyToAttack
		{
			get
			{
				return Time.time >= this.lastAttackTimestamp + this.attackCooldown;
			}
		}

		void Awake()
		{
		}

		private void Start()
		{
			this.lastAttackTimestamp = UnityEngine.Random.Range( -this.attackCooldown, 0.0f );
			this.factionMember = this.GetComponent<FactionMember>();
		}

		void Update()
		{
			// If it's not usable - return, don't attack.
			if( this.ssObject is IUsableToggle && !(this.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}

			if( this.__target != null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.rotation = Quaternion.LookRotation( (this.__target.transform.position - this.traversibleSubObjects[i].transform.position).normalized, this.transform.up );
				}
			}

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
			MeleeModuleData data = new MeleeModuleData();
			
			if( this.target != null )
			{
				data.targetGuid = this.target.GetComponent<SSObject>().guid.Value;
			}
			return data;
		}

		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is MeleeModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is MeleeModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			MeleeModuleDefinition def = (MeleeModuleDefinition)_def;
			MeleeModuleData data = (MeleeModuleData)_data;

			this.icon = def.icon;
			this.searchRange = def.attackRange;
			
			DamageSource damageSource = new DamageSource( def.damageType, def.damage, def.armorPenetration );
			this.damageSource = damageSource;

			this.attackCooldown = def.attackCooldown;
			this.attackSoundEffect = def.attackSoundEffect;

			this.traversibleSubObjects = new SubObject[def.traversibleSubObjects.Length];
			for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
			{
				SubObject trav = this.ssObject.GetSubObject( def.traversibleSubObjects[i] );
				this.traversibleSubObjects[i] = trav ?? throw new Exception( "Can't find Sub-Object with Id of '" + def.traversibleSubObjects[i].ToString( "D" ) + "'." );
			}

			if( data.targetGuid != null )
			{
				this.TrySetTarget( Main.GetSSObject( data.targetGuid.Value ).GetComponent<Damageable>() );
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