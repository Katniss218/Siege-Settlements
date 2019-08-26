﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	/// <summary>
	/// Represents objects that can be damaged.
	/// </summary>
	public class Damageable : MonoBehaviour
	{
		/// <summary>
		/// Fires when the 'health' value is changed.
		/// </summary>
		public UnityEvent onHealthChange = new UnityEvent();

		/// <summary>
		/// Fires when the damageable is killed ('health' value is less or equal to 0, or by using Die()).
		/// </summary>
		public UnityEvent onDeath = new UnityEvent();


		/// <summary>
		/// Current health value of this damageable.
		/// </summary>
		[SerializeField]
		private float __health;
		/// <summary>
		/// Gets or sets the health value. Calls 'onHealthChange'. If set to 0, the damageable will die, calling 'onDeath'.
		/// </summary>
		public float health
		{
			get
			{
				return this.__health;
			}
			set
			{
				
				if( value > this.healthMax )
				{
					throw new ArgumentOutOfRangeException( "Can't set the health to more than max health." );
				}
				// Make sure that health can't go below 0.
				if( value < 0 )
				{
					value = 0;
				}
				this.__health = value;
				this.onHealthChange?.Invoke();

				// If the health is 0, kill the damageable.
				if( this.__health == 0 )
				{
					this.Die();
				}
			}
		}

		/// <summary>
		/// Maximum health of this damageable.
		/// </summary>
		[SerializeField]
		private float __healthMax;
		public float healthMax
		{
			get
			{
				return this.__healthMax;
			}
			set
			{
				this.__healthMax = value;
			}
		}

		/// <summary>
		/// Returns the percentage of the health of this damageable.
		/// </summary>
		public float healthPercent
		{
			get
			{
				return this.health / this.healthMax;
			}
			set
			{
				if( value < 0 )
				{
					throw new ArgumentOutOfRangeException( "Can't set the health percentage to less than 0." );
				}
				if( value > 1 )
				{
					throw new ArgumentOutOfRangeException( "Can't set the health percentage to more than 1." );
				}
				this.health = value * this.healthMax;
			}
		}
		
		/// <summary>
		/// The armor of the damageable.
		/// </summary>
		public Armor armor { get; set; }
		
		/// <summary>
		/// Heals this damageable to full health.
		/// </summary>
		public virtual void Heal()
		{
			this.health = this.healthMax;
		}

		/// <summary>
		/// Heals this damageable by the specified amount.
		/// </summary>
		/// <param name="amount">The amount of health to restore.</param>
		public virtual void Heal( float amount )
		{
			if( amount <= 0 )
			{
				throw new ArgumentOutOfRangeException( "Can't heal for less than 1 health." );
			}
			if( this.health + amount > this.healthMax )
			{
				this.health = this.healthMax;
			}
			else
			{
				this.health += amount;
			}
		}

		/// <summary>
		/// Makes the damageable take an amount of damage, without applying any modifiers to it.
		/// </summary>
		/// <param name="amount">The raw amount of damage to take.</param>
		public virtual void TakeDamageUnscaled( float amount )
		{
			if( amount <= 0 )
			{
				throw new ArgumentOutOfRangeException( "Can't take 0 or less damage" );
			}
			this.health -= amount;
			/*if( this.health <= 0 )
			{
				this.Die();
			}*/
		}

		/// <summary>
		/// makes the damageable take an amount of damage, using the scaling reduction formula.
		/// </summary>
		/// <param name="type">The type of damage taken.</param>
		/// <param name="amount">The raw amount of damage to take.</param>
		/// <param name="armorPenetration">The armor penetration of the attacker.</param>
		public virtual void TakeDamage( DamageType type, float amount, float armorPenetration )
		{
			if( amount <= 0 )
			{
				throw new ArgumentOutOfRangeException( "Can't take 0 or less damage" );
			}

			float reducedDamage = this.armor.CalculateReducedDamage( amount, type, armorPenetration ); 

			this.health -= reducedDamage;
			/*if( this.health <= 0 )
			{
				this.Die();
			}*/
		}

		/// <summary>
		/// Makes this damageable die.
		/// </summary>
		public virtual void Die()
		{
			Destroy( this.gameObject );
			this.onDeath?.Invoke();
		}
	}
}