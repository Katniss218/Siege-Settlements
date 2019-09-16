using System;

namespace SS.ResourceSystem.Payment
{
	public interface IPaymentProgress
	{
		/// <summary>
		/// Returns the progress of whatever is being done.
		/// </summary>
		//Func<float> progress { get; }

		Func<bool> IsDone { get; }

		/// <summary>
		/// Returns the maximum amount of specific resource, that the progress wants.
		/// </summary>
		int GetWantedAmount( string resourceId );
	}
}