using System;

namespace SS.ResourceSystem.Payment
{
	public interface IPaymentProgress
	{
		/// <summary>
		/// Checks if the progress is 100%.
		/// </summary>
		Func<bool> IsDone { get; }

		/// <summary>
		/// Returns the maximum amount of specific resource, that the progress wants.
		/// </summary>
		int GetWantedAmount( string resourceId );
	}
}