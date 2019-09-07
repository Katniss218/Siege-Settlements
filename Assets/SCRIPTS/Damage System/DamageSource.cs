using UnityEngine;

namespace SS
{
	public sealed class DamageSource
	{
		/// <summary>
		/// Used to tell the GetRandomizedDamage how much it can deviate from the default damage value.
		/// </summary>
		public const float RANDOM_DEVIATION = 0.25f;

		/// <summary>
		/// The damage type of this DamageSource.
		/// </summary>
		public DamageType damageType;

		/// <summary>
		/// The damage value of this DamageSource.
		/// </summary>
		public float damage;

		/// <summary>
		/// The armor penetration of this DamageSource - how much of the target's armor should be ignored.
		/// </summary>
		public float armorPenetration;

		public DamageSource( DamageType damageType, float damage, float armorPenetration )
		{
			this.damageType = damageType;
			this.damage = damage;
			this.armorPenetration = armorPenetration;
		}

		/// <summary>
		/// Calculates a randomized damage value, randomizes it by +- RANDOM_DEVIATION.
		/// </summary>
		public float GetRandomizedDamage()
		{
			return this.damage * Random.Range( 1.0f - RANDOM_DEVIATION, 1.0f + RANDOM_DEVIATION );
		}
	}
}