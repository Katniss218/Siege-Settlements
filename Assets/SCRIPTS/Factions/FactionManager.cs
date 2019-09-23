namespace SS
{
	public static class FactionManager
	{
		public const int PLAYER = 0;

		/// <summary>
		/// The currently registered factions.
		/// </summary>
		public static FactionData[] factions { get; private set; }

		/// <summary>
		/// Sets the factions.
		/// </summary>
		public static void SetFactions( FactionData[] factions )
		{
			FactionManager.factions = factions;
		}
	}
}