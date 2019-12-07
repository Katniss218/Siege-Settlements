﻿namespace SS.Objects.Modules
{
	public interface IAttackModule
	{
		/// <summary>
		/// Returns the max view distance of the target finder.
		/// </summary>
		float searchRange { get; }
		
		bool isReadyToAttack { get; }

		Targeter targeter { get; }
	}
}