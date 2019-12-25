using SS.Levels.SaveStates;
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
		public Targeter targeter { get; private set; }


		public float damage;
		public float? damageOverride;
		public float armorPenetration;
		public DamageType damageType;

		public float attackCooldown;
		public AudioClip attackSoundEffect;

		private float lastAttackTimestamp;

		public bool isReadyToAttack
		{
			get
			{
				return Time.time >= (this.lastAttackTimestamp + this.attackCooldown);
			}
		}

		private bool isReady2 = false;

		public SubObject[] traversibleSubObjects { get; set; }


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		void Awake()
		{
			this.targeter = new Targeter( ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK, this.ssObject as SSObjectDFS );

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
			this.lastAttackTimestamp = UnityEngine.Random.Range( -this.attackCooldown, 0.0f ) + Time.time;
		}

		void Update()
		{
			// If it's not usable - return, don't attack.
			if( this.ssObject is IUsableToggle && !((IUsableToggle)this.ssObject).IsUsable() )
			{
				return;
			}

			if( this.ssObject is Unit && ((Unit)this.ssObject).isInsideHidden )
			{
				return;
			}

			if( this.targeter.target != null )
			{
				for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
				{
					this.traversibleSubObjects[i].transform.rotation = Quaternion.LookRotation( (this.targeter.target.transform.position - this.traversibleSubObjects[i].transform.position).normalized, this.transform.up );
				}
			}

			if( this.isReady2 )
			{
				if( this.targeter.target == null )
				{
					return;
				}

				this.Attack( this.targeter.target );
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
			float damage = this.damageOverride == null ? this.damage : this.damageOverride.Value;

			target.TakeDamage( this.damageType, DamageUtils.GetRandomized( damage, DamageUtils.RANDOM_DEVIATION ), this.armorPenetration );
			AudioManager.PlaySound( this.attackSoundEffect );
			this.lastAttackTimestamp = Time.time;
			this.isReady2 = false;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override ModuleData GetData()
		{
			MeleeModuleData data = new MeleeModuleData();

			if( this.targeter.target != null )
			{
				data.targetGuid = this.targeter.target.GetComponent<SSObject>().guid;
			}
			if( this.damageOverride != null )
			{
				data.damageOverride = this.damageOverride;
			}
			return data;
		}

		public override void SetData( ModuleData _data )
		{
			if( !(_data is MeleeModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			MeleeModuleData data = (MeleeModuleData)_data;

			if( data.targetGuid != null )
			{
				this.targeter.target = SSObject.Find( data.targetGuid.Value ) as SSObjectDFS;
			}
			if( data.damageOverride != null )
			{
				this.damageOverride = data.damageOverride;
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
			Gizmos.DrawWireSphere( this.transform.position, this.attackRange );
		}
#endif
	}
}