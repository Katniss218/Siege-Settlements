﻿using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using SS.Objects.Units;
using System;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class MeleeModule : SSModule, IAttackModule
	{
		public const string KFF_TYPEID = "melee";

		public float attackRange { get; set; }

		private SSObjectDFC __target;
		public SSObjectDFC target
		{
			get
			{
				return this.__target;
			}
			set
			{
				this.__target = value;
				if( value == null )
				{
					for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
					{
						this.traversibleSubObjects[i].transform.localRotation = this.traversibleSubObjects[i].defaultRotation;
					}
				}
			}
		}

		public float damage;
		public float? damageOverride;
		public float armorPenetration;
		public DamageType damageType;

		public float attackCooldown;
		public AudioClip attackSoundEffect;

		private float lastAttackTimestamp;
		private SSObjectDFC __factionMemberSelf = null;
		private SSObjectDFC factionMemberSelf
		{
			get
			{
				if( __factionMemberSelf == null )
				{
					__factionMemberSelf = this.ssObject as SSObjectDFC;
				}
				return __factionMemberSelf;
			}
		}

		public bool isReadyToAttack
		{
			get
			{
				return Time.time >= (this.lastAttackTimestamp + this.attackCooldown);
			}
		}

		private bool isReady2 = false;

		public SubObject[] traversibleSubObjects { get; set; }


		public void FindTargetClosest()
		{
			this.target = Targeter.FindTargetClosest( this.transform.position, this.attackRange, this.factionMemberSelf, false );
		}

		public void TrySetTarget( SSObjectDFC target )
		{
			this.target = Targeter.TrySetTarget( this.transform.position, this.attackRange, this.factionMemberSelf, target, false );
		}

		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		void Start()
		{
			this.lastAttackTimestamp = UnityEngine.Random.Range( -this.attackCooldown, 0.0f ) + Time.time;
		}

		void Update()
		{
			// If it's not usable - return, don't attack.
			if( this.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.ssObject).isUsable )
			{
				return;
			}

			if( this.ssObject is Unit && ((Unit)this.ssObject).isInsideHidden )
			{
				return;
			}

			if( this.target != null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.rotation = Quaternion.LookRotation( (this.target.transform.position - this.traversibleSubObjects[i].transform.position).normalized, this.transform.up );
				}
			}

			if( this.isReady2 )
			{
				if( this.target == null )
				{
					return;
				}

				this.Attack( this.target );
			}

			if( this.isReadyToAttack )
			{
				this.isReady2 = true;
			}
		}

		/// <summary>
		/// Forces MeleeComponent to shoot at the target (assumes target != null).
		/// </summary>
		public void Attack( IDamageable target )
		{
			SSObject targetObj = ((SSObject)target);
			float hitChance = Main.CalculateHitChance( targetObj, this.transform.position.y );
			if( Main.IsHit( hitChance ) )
			{
				float damage = this.damageOverride == null ? this.damage : this.damageOverride.Value;

				float damageRand = DamageUtils.GetRandomized( damage, DamageUtils.RANDOM_DEVIATION );

				target.TakeDamage( this.damageType, damageRand, this.armorPenetration );

				AudioManager.PlaySound( this.attackSoundEffect, this.transform.position );
			}
			else
			{
				AudioManager.PlaySound( this.attackSoundEffect, this.transform.position, 0.5f, 1.5f );
			}
			this.lastAttackTimestamp = Time.time;
			this.isReady2 = false;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override SSModuleData GetData()
		{
			MeleeModuleData data = new MeleeModuleData();

			if( this.target != null )
			{
				data.targetGuid = this.target.GetComponent<SSObject>().guid;
			}
			if( this.damageOverride != null )
			{
				data.damageOverride = this.damageOverride;
			}
			return data;
		}

		public override void SetData( SSModuleData _data )
		{
			MeleeModuleData data = ValidateDataType<MeleeModuleData>( _data );

			if( data.targetGuid != null )
			{
				this.target = SSObject.Find( data.targetGuid.Value ) as SSObjectDFC;
			}
			if( data.damageOverride != null )
			{
				this.damageOverride = data.damageOverride;
			}
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