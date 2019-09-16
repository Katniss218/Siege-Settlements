namespace SS
{
	public static class ObjectLayer
	{
		public const int TERRAIN = 10;

		public const int UNITS = 20;
		public const int BUILDINGS = 21;
		public const int PROJECTILES = 22;
		public const int HEROES = 23;
		public const int EXTRAS = 24; // TODO - resource deposits are just extras with custom inventory.

		public const int TERRAIN_MASK = 1 << TERRAIN;
		public const int UNITS_MASK = 1 << UNITS;
		public const int BUILDINGS_MASK = 1 << BUILDINGS;
		public const int PROJECTILES_MASK = 1 << PROJECTILES;
		public const int HEROES_MASK = 1 << HEROES;
		public const int EXTRAS_MASK = 1 << EXTRAS;
	}
}