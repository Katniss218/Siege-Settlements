using SS.Diplomacy;
using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using System;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class MeleeModule : SSModule, ITargeterModule
	{
		public const string KFF_TYPEID = "melee";

		public float searchRange { get; set; }

		// it's the target finder.

		private Targeter targeter;
		public Damageable TrySetTarget()
		{
			return this.targeter.TrySetTarget( this.transform.position );
		}

		public Damageable TrySetTarget( Damageable target )
		{
			return this.targeter.TrySetTarget( this.transform.position, target );
		}

		public DamageSource damageSource;
		public float attackCooldown;
		public AudioClip attackSoundEffect;
		
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
			this.targeter = new Targeter( this.searchRange, ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK, this.GetComponent<FactionMember>() );

			this.targeter.onTargetReset += () =>
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
				}
			};
		}

		void Start()
		{
			this.lastAttackTimestamp = UnityEngine.Random.Range( -this.attackCooldown, 0.0f );
		}

		void Update()
		{
			// If it's not usable - return, don't attack.
			if( this.ssObject is IUsableToggle && !(this.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}

			if( !Targeter.CanTarget( this.targeter.factionMember, this.targeter.target, this.transform.position, this.searchRange ) )
			{
				this.targeter.target = null;
			}

			if( this.targeter.target != null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.rotation = Quaternion.LookRotation( (this.targeter.target.transform.position - this.traversibleSubObjects[i].transform.position).normalized, this.transform.up );
				}
			}

			if( this.isReadyToAttack )
			{
				// Get target, if current target is not targetable or no target is present - try to find a suitable one.
				if( !Targeter.CanTarget( this.targeter.factionMember, this.targeter.target, this.transform.position, this.searchRange ) )
				{
					this.targeter.TrySetTarget( this.transform.position );
				}
				
				if( this.targeter.target == null )
				{
					return;
				}
				
				this.Attack( this.targeter.target );
				AudioManager.PlaySound( this.attackSoundEffect );
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



		public override ModuleData GetData()
		{
			MeleeModuleData data = new MeleeModuleData();
			
			if( this.targeter.target != null )
			{
				data.targetGuid = this.targeter.target.GetComponent<SSObject>().guid.Value;
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

			this.targeter.searchRange = this.searchRange;
			if( data.targetGuid != null )
			{
				this.targeter.TrySetTarget( this.transform.position, Main.GetSSObject( data.targetGuid.Value ).GetComponent<Damageable>() );
			}
		}
		
#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if( this.targeter.target != null )
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine( this.transform.position, this.targeter.target.transform.position );
				Gizmos.DrawSphere( this.targeter.target.transform.position, 0.125f );
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