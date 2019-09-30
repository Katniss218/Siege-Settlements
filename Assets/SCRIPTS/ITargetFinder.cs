using SS.Factions;
using System;

namespace SS
{
	public interface ITargetFinder
	{
		/// <summary>
		/// Returns the found target, null if none found.
		/// </summary>
		Damageable GetTarget();

		/// <summary>
		/// Used to check if the two faction members are hostile towards one naother.
		/// </summary>
		Func<FactionMember, FactionMember, bool> canTarget { get; set; }
	}
}