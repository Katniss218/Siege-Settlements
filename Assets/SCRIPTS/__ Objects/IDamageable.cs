using Katniss.ModifierAffectedValues;
using UnityEngine.Events;

namespace SS.Objects
{
	public interface IDamageable
	{
		float health { get; set; }

		//float healthMax { get; set; }
		FloatM healthMax { get; }

		float healthPercent { get; set; }

		Armor armor { get; set; }

		void TakeDamage( DamageType type, float amount, float armorPenetration );

		void Die();

		_UnityEvent_float onHealthChange { get; }

		UnityEvent onDeath { get; }
	}
}