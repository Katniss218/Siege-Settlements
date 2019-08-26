using System;
using System.Linq;

namespace SS
{
	/// <summary>
	/// Represents each of possible damage types.
	/// </summary>
	public enum DamageType : byte
	{
		Slash,
		Pierce,
		Concussion
	}

	public static class DamageTypeExtensions
	{
		static int? numTypesCache = null;

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