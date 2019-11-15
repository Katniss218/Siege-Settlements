﻿using SS.Buildings;
using SS.Diplomacy;
using SS.Levels.SaveStates;
using System;
using UnityEngine;

namespace SS.Modules
{
	public class MeleeModule : Module, ITargetFinder
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
						this.traversibleSubObjects[i].localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
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

			FactionMember targetFactionMember = target.GetComponent<FactionMember>();
			if( !FactionMember.CanTargetAnother( this.factionMember, targetFactionMember ) )
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
					this.traversibleSubObjects[i].localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
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
					this.traversibleSubObjects[i].localRotation = this.traversibleSubObjects[i].GetComponent<SubObject>().defaultRotation;
				}
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
				if( !FactionMember.CanTargetAnother( this.factionMember, targetFactionMember ) )
				{
					continue;
				}

				// Disregard potential targets, if the overlap is present, but the center is outside.
				if( Vector3.Distance( this.transform.position, potentialTarget.transform.position ) >= searchRange )
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

		private bool isBuilding;
		private Damageable damageableSelf;

		private Transform[] traversibleSubObjects { get; set; }

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
			this.isBuilding = Building.IsValid( this.gameObject );
			this.factionMember = this.GetComponent<FactionMember>();
			this.damageableSelf = this.GetComponent<Damageable>();
		}

		void Update()
		{
			// If it's a building and it's not usable - return, don't attack.
			if( this.isBuilding && !Building.IsUsable( this.damageableSelf ) )
			{
				return;
			}

			if( this.__target != null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].rotation = Quaternion.LookRotation( (this.__target.transform.position - this.traversibleSubObjects[i].transform.position).normalized, this.transform.up );
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
				data.targetGuid = Main.GetGuid( this.target.gameObject );
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

			this.searchRange = def.attackRange;
			
			DamageSource damageSource = new DamageSource( def.damageType, def.damage, def.armorPenetration );
			this.damageSource = damageSource;

			this.attackCooldown = def.attackCooldown;
			this.attackSoundEffect = def.attackSoundEffect;

			this.traversibleSubObjects = new Transform[def.traversibleSubObjects.Length];
			for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
			{
				SSObject self = this.GetComponent<SSObject>();

				SubObject trav = self.GetSubObject( def.traversibleSubObjects[i] );
				if( trav == null )
				{
					throw new Exception( "Can't find Sub-Object with Id of '" + def.traversibleSubObjects[i].ToString( "D" ) + "'." );
				}
				this.traversibleSubObjects[i] = trav.transform;
			}

			if( data.targetGuid != null )
			{
				this.TrySetTarget( Main.GetGameObject( data.targetGuid.Value ).GetComponent<Damageable>() );
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