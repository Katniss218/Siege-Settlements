namespace SS.Objects.Units
{
	/// <summary>
	/// Determines inside which slots the unit can fit.
	/// </summary>
	public enum PopulationSize : byte
	{
		INVALID = 0, // was default all along.
		x1 = 1,
		x2 = 2,
		x4 = 4,
		x8 = 8
	}
}