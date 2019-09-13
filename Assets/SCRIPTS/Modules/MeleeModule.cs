﻿using UnityEngine;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	[RequireComponent( typeof( ITargetFinder ) )]
	public class MeleeModule : Module
	{
		public DamageSource damageSource;
		public ITargetFinder targetFinder;

		// TODO - this is not used anywhere except target finder.
		public float attackRange;
		public float attackCooldown;

		public AudioClip attackSoundEffect;

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
		}

		private void Start()
		{
			if( this.damageSource == null )
			{
				Debug.LogError( "There's no damage source hooked up to this melee module." );
			}
			if( this.targetFinder == null )
			{
				Debug.LogError( "There's no target finder hooked up to this melee module." );
			}
			this.lastAttackTimestamp = Random.Range( -this.attackCooldown, 0.0f );
		}

		void Update()
		{
			if( this.isReadyToAttack )
			{
				Damageable target = this.targetFinder.GetTarget();

				if( target != null )
				{
					this.Attack( target );
					AudioManager.PlayNew( this.attackSoundEffect, 1.0f, 1.0f );
				}
			}
		}


		public static void AddTo( GameObject obj, MeleeModuleDefinition def )
		{
			TargetFinder finder = obj.AddComponent<TargetFinder>();

			finder.canTarget = FactionMember.CanTargetCheck;
			finder.searchRange = def.attackRange;


			DamageSource damageSource = new DamageSource( def.damageType, def.damage, def.armorPenetration );

			MeleeModule melee = obj.AddComponent<MeleeModule>();
			melee.damageSource = damageSource;
			melee.targetFinder = finder;
			melee.attackCooldown = def.attackCooldown;
			melee.attackRange = def.attackRange;
			melee.attackSoundEffect = def.attackSoundEffect.Item2;
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

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere( this.transform.position, this.attackRange );
		}
#endif
	}
}