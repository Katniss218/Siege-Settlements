using SS.Diplomacy;
using System;

namespace SS
{
	public interface ITargetFinder
	{
		/// <summary>
		/// Returns the current target, returns null if the target is no longer targetable.
		/// </summary>
		Damageable target { get; }

		/// <summary>
		/// Returns the max view distance of the target finder.
		/// </summary>
		float searchRange { get; }

		/// <summary>
		/// Used to check if the two faction members are hostile towards one naother.
		/// </summary>
		Func<FactionMember, FactionMember, bool> canTarget { get; set; }


		/// <summary>
		/// Tries to set the target to any damageable.
		/// </summary>
		Damageable TrySetTarget();

		/// <summary>
		/// Tries to set the target to the specified damageable.
		/// </summary>
		/// <param name="target">The object to try to target.</param>
		Damageable TrySetTarget( Damageable target );
	}
}