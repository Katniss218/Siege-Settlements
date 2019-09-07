using KFF;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Represents an armor, that can reduce incoming damage by a specified amount.
	/// </summary>
	public class Armor : IKFFSerializable
	{
		/// <summary>
		/// Contains percentage reduction values for each type of damage. You can use DamageType enum to index it, to get the correcponding armor values.
		/// </summary>
		public float[] values { get; private set; }

		public Armor()
		{
			this.values = new float[DamageTypeExtensions.GetNumTypes()];
			for( int i = 0; i < values.Length; i++ )
			{
				values[i] = 0.0f;
			}
		}

		private float GetMultiplier( float armor, float penetration )
		{
			// The total of 0.0 means 0% reduction, 1.0 means 100% reduction (the total is calculated: victim's armor - attacker's armor penetration, never above 1.0).
			// so if you have 2.0 armor and attacker has 1.5 penetration, there will be 50% damage reduction.

			float mult = 1 - (armor - penetration);

			// If the damage somehow ended up greater than before reduction, clamp it, and log warning.
			if( mult > 1 )
			{
				Debug.LogWarning( "CalculateReducedDamage: The reduced damage was greater than before reduction. Clamping the damage multiplier to 1." );
				mult = 1;
			}
			return mult;
		}

		/// <summary>
		/// Calculates reduced damage, based on the armor's current values and the parameters of the incoming damage.
		/// </summary>
		/// <param name="incomingDamage">The amount of incoming damage (the raw damage, before any reductions).</param>
		/// <param name="damageType">The type of incoming damage.</param>
		/// <param name="armorPenetration">The amount of armor penetration of the incoming damage.</param>
		public float CalculateReducedDamage( float incomingDamage, DamageType damageType, float armorPenetration )
		{
			return incomingDamage * this.GetMultiplier( this.values[(int)damageType], armorPenetration);
		}
		
		public void DeserializeKFF( KFFSerializer serializer )
		{
			float[] values = serializer.ReadFloatArray( "ArmorValues" );
			if( values.Length != DamageTypeExtensions.GetNumTypes() )
			{
				throw new System.Exception( "Invalid Armor array, the number of damage types doesn't match the real number." );
			}
			this.values = values;
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloatArray( "", "ArmorValues", this.values );
		}
	}
}