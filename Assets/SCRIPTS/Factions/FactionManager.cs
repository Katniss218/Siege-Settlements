namespace SS
{
	public static class FactionManager
	{
		/// <summary>
		/// The currently registered factions.
		/// </summary>
		public static Faction[] factions { get; private set; }

		/// <summary>
		/// Sets the factions.
		/// </summary>
		public static void SetFactions( Faction[] factions )
		{
			FactionManager.factions = factions;
		}
	}
}