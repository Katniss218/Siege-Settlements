using UnityEngine;

namespace SS
{
	public static class DamageUtils
	{
		public const float RANDOM_DEVIATION = 0.25f;

		public static float GetRandomized( float damage, float maxDeviationPercent )
		{
			return damage * Random.Range( 1.0f - maxDeviationPercent, 1.0f + maxDeviationPercent );
		}
	}
}
