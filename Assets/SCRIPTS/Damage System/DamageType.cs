using System;
using System.Linq;

namespace SS
{
	/// <summary>
	/// Represents every valid damage type.
	/// </summary>
	public enum DamageType : byte
	{
		Slash = 0,
		Pierce = 1,
		Concussion = 2
	}

	public static class DamageTypeExtensions
	{
		// Caches the number of distinct damage types (since it shouldn't change after compilation), for faster lookup.
		private static int? numTypesCache = null;

		/// <summary>
		/// Returns the number of defined damage types.
		/// </summary>
		public static int GetNumTypes()
		{
			if( numTypesCache == null )
			{
				numTypesCache = Enum.GetValues( typeof( DamageType ) ).Cast<DamageType>().Distinct().Count();
			}

			return numTypesCache.Value;
		}
	}
}