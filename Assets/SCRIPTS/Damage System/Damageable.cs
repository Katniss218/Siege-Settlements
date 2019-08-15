using UnityEngine;

namespace SS
{
	public class Damageable : MonoBehaviour
	{
		/// <summary>
		/// Current health value of this damageable.
		/// </summary>
		public float health { get; protected set; }
		/// <summary>
		/// Maximum health of this damageable.
		/// </summary>
		public float healthMax { get; protected set; }

		/// <summary>
		/// Returns the percentage of the health of this damageable (Read only).
		/// </summary>
		public float healthPercent { get { return this.health / this.healthMax; } }
		
		/// <summary>
		/// Percentage reduction of the slash-type damage.
		/// </summary>
		public float slashArmor { get; protected set; }
		/// <summary>
		/// Percentage reduction of the pierce-type damage.
		/// </summary>
		public float pierceArmor { get; protected set; }
		/// <summary>
		/// Percentage reduction of the concussion-type damage.
		/// </summary>
		public float concussionArmor { get; protected set; }


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
		
		public virtual void TakeDamage( DamageType type, float amount, float armorPenetration )
		{
			if( amount < 0 )
			{
				throw new System.Exception( "Can't take less than 0 damage" );
			}
			float mult = 0;
			if( type == DamageType.Slash )
			{
				mult = 1 - (this.slashArmor - armorPenetration);
			}
			if( type == DamageType.Pierce )
			{
				mult = 1 - (this.pierceArmor - armorPenetration);
			}
			if( type == DamageType.Concussion )
			{
				mult = 1 - (this.concussionArmor - armorPenetration);
			}
			if( mult > 1 )
			{
				mult = 1;
			}
			this.health -= amount * mult;
			if( this.health <= 0 )
			{
				this.Die();
			}
		}

		public virtual void Die()
		{
			Destroy( this.gameObject );
		}
	}
}