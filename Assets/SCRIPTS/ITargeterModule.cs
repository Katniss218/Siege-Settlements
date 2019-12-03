namespace SS
{
	public interface ITargeterModule
	{
		/// <summary>
		/// Returns the max view distance of the target finder.
		/// </summary>
		float searchRange { get; }
		

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