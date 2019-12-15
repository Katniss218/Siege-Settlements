namespace SS.Objects.Units
{
	/// <summary>
	/// Determines to which slots the unit can fit.
	/// </summary>
	public enum UnitPopulationSize : byte
	{
		/// <summary>
		/// Unit can fit inside 'x1' slots (single)
		/// </summary>
		x1 = 1,

		/// <summary>
		/// Unit can fit inside 'x2' and 'x1' slots (double|single).
		/// </summary>
		x2 = 2,


		// Can't fit in slots - too big...

		x4 = 4,
		x8 = 8
	}
}