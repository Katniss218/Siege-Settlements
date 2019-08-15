using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	public class Damageable : MonoBehaviour
	{
		public class _UnityEventDamageable : UnityEvent<Damageable> { }

		public _UnityEventDamageable onHealthChange = new _UnityEventDamageable();
		public _UnityEventDamageable onDeath = new _UnityEventDamageable();

		/// <summary>
		/// Current health value of this damageable.
		/// </summary>
		public float health;
		/// <summary>
		/// Maximum health of this damageable.
		/// </summary>
		public float healthMax;

		/// <summary>
		/// Returns the percentage of the health of this damageable (Read only).
		/// </summary>
		public float healthPercent { get { return this.health / this.healthMax; } }

		/// <summary>
		/// Percentage reduction of the slash-type damage.
		/// </summary>
		public float slashArmor;
		/// <summary>
		/// Percentage reduction of the pierce-type damage.
		/// </summary>
		public float pierceArmor;
		/// <summary>
		/// Percentage reduction of the concussion-type damage.
		/// </summary>
		public float concussionArmor;


		public virtual void Heal()
		{
			this.health = this.healthMax;
			this.onHealthChange?.Invoke( this );
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
			this.onHealthChange?.Invoke( this );
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

			this.onHealthChange?.Invoke( this );
		}

		public virtual void Die()
		{
			Destroy( this.gameObject );
			this.onDeath?.Invoke( this );
		}
	}
}