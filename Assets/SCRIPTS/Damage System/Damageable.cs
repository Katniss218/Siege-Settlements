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
		/// Current health value of this damageable (Read only).
		/// </summary>
		[SerializeField]
		private float __health;
		public float health
		{
			get
			{
				return this.__health;
			}
			private set
			{
				this.__health = value;
				//onHealthChange?.Invoke();
			}
		}

		/// <summary>
		/// Maximum health of this damageable (Read only).
		/// </summary>
		[SerializeField]
		private float __healthMax;
		public float healthMax
		{
			get
			{
				return this.__healthMax;
			}
			private set
			{
				this.__healthMax = value;
			}
		}

		/// <summary>
		/// Returns the percentage of the health of this damageable (Read only).
		/// </summary>
		public float healthPercent
		{
			get
			{
				return this.health / this.healthMax;
			}
			private set
			{
				this.health = value * this.healthMax;
			}
		}
		
		/// <summary>
		/// The armor of the damageable.
		/// </summary>
		public Armor armor;
		/// <summary>
		/// Percentage reduction of the slash-type damage.
		/// </summary>
		//public float slashArmor;
		/// <summary>
		/// Percentage reduction of the pierce-type damage.
		/// </summary>
		//public float pierceArmor;
		/// <summary>
		/// Percentage reduction of the concussion-type damage.
		/// </summary>
		//public float concussionArmor;

		/// <summary>
		/// Sets the damageable's max health to the specified value.
		/// </summary>
		/// <param name="value">The value to set the max health to.</param>
		/// <param name="heal">If true, the damageable will get healed to full health.</param>
		public virtual void SetMaxHealth( float value, bool heal )
		{
			this.healthMax = value;
			if( heal )
			{
				this.Heal();
			}
		}

		/// <summary>
		/// Sets the health to the specified value. Kills the damageable if value == 0.
		/// </summary>
		/// <param name="value">The health (0-healthMax).</param>
		public virtual void SetHealth( float value )
		{
			if( value < 0 )
			{
				throw new System.Exception( "Can't set the health to less than 0." );
			}
			if( value > this.healthMax )
			{
				throw new System.Exception( "Can't set the health to more than max health." );
			}

			this.health = value;
			onHealthChange?.Invoke();
			if( value == 0 )
			{
				this.Die();
			}
		}

		/// <summary>
		/// Sets the health percentage to the specified value. Kills the damageable if value == 0.
		/// </summary>
		/// <param name="value">The health percentage (0-1).</param>
		public virtual void SetHealthPercent( float value )
		{
			if( value < 0 )
			{
				throw new System.Exception( "Can't set the health percentage to less than 0." );
			}
			if( value > 1 )
			{
				throw new System.Exception( "Can't set the health percentage to more than 1." );
			}

			this.healthPercent = value;
			onHealthChange?.Invoke();
			if( value == 0 )
			{
				this.Die();
			}
		}

		/// <summary>
		/// Heals this damageable to full health.
		/// </summary>
		public virtual void Heal()
		{
			this.health = this.healthMax;
			this.onHealthChange?.Invoke();
		}

		/// <summary>
		/// Heals this damageable by the specified amount.
		/// </summary>
		/// <param name="amount">The amount of health to restore.</param>
		public virtual void Heal( float amount )
		{
			if( amount <= 0 )
			{
				throw new System.Exception( "Can't heal for less than 1 health." );
			}
			if( this.health + amount > this.healthMax )
			{
				this.health = this.healthMax;
			}
			else
			{
				this.health += amount;
			}
			this.onHealthChange?.Invoke();
		}

		/// <summary>
		/// Makes the damageable take an amount of damage, without applying any modifiers to it.
		/// </summary>
		/// <param name="amount">The raw amount of damage to take.</param>
		public virtual void TakeDamageUnscaled( float amount )
		{
			if( amount <= 0 )
			{
				throw new System.Exception( "Can't take 0 or less damage" );
			}
			this.health -= amount;
			this.onHealthChange?.Invoke();
			if( this.health <= 0 )
			{
				this.Die();
			}
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
				throw new System.Exception( "Can't take 0 or less damage" );
			}

			float reducedDamage = this.armor.CalculateReducedDamage( amount, type, armorPenetration ); 

			this.health -= reducedDamage;
			this.onHealthChange?.Invoke();
			if( this.health <= 0 )
			{
				this.Die();
			}
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