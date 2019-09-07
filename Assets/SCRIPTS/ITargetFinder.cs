using System;

namespace SS
{
	public interface ITargetFinder
	{
		/// <summary>
		/// Used to check if the two faction members are hostile towards one naother.
		/// </summary>
		Func<FactionMember, FactionMember, bool> canTarget { get; set; }

		/// <summary>
		/// Used to return a valid target. Should be null if none found.
		/// </summary>
		/// <param name="searchRange">The max distance for searching.</param>
		Damageable FindTarget( float searchRange );
	}
}