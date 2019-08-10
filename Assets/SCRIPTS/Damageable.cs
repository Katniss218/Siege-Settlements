using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Represents any object that can be damaged by other objects.
	/// </summary>
	public abstract class Damageable : MonoBehaviour
	{
		/// <summary>
		/// Current health value of this damageable.
		/// </summary>
		public abstract float health { get; protected set; }
		/// <summary>
		/// Maximum health of this damageable.
		/// </summary>
		public abstract float healthMax { get; protected set; }

		/// <summary>
		/// Returns the percentage of the health of this damageable (Read only).
		/// </summary>
		public float healthPercent { get { return this.health / this.healthMax; } }
		
		public virtual void Heal()
		{
			this.health = this.healthMax;
		}

		public virtual void Heal( float amount )
		{
			if( amount <= 0 )
			{
				throw new System.Exception( "Can't heal for less than 1 health." );
			}
			this.health += amount;
			if( this.health > this.healthMax )
			{
				this.health = this.healthMax;
			}
		}
		
		public abstract void TakeDamage( DamageType type, float amount, float armorPenetration );

		public abstract void Die();
	}
}