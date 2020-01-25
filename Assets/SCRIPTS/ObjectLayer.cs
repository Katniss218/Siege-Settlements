namespace SS
{
	public static class ObjectLayer
	{
		/// <summary>
		/// This layer is used by terrain objects.
		/// </summary>
		public const int TERRAIN = 10;

		public const int TERRAIN_MASK = 1 << TERRAIN;


		/// <summary>
		/// Objects that are in this layer are defined as units.
		/// </summary>
		public const int UNITS = 20;

		/// <summary>
		/// Objects that are in this layer are defined as buildings.
		/// </summary>
		public const int BUILDINGS = 21;

		/// <summary>
		/// Objects that are in this layer are defined as projectiles.
		/// </summary>
		public const int PROJECTILES = 22;

		/// <summary>
		/// Objects that are in this layer are defined as heroes.
		/// </summary>
		public const int HEROES = 23;

		/// <summary>
		/// Objects that are in this layer are defined as extras.
		/// </summary>
		public const int EXTRAS = 24;


		public const int UNITS_MASK = 1 << UNITS;
		public const int BUILDINGS_MASK = 1 << BUILDINGS;
		public const int PROJECTILES_MASK = 1 << PROJECTILES;
		public const int HEROES_MASK = 1 << HEROES;
		public const int EXTRAS_MASK = 1 << EXTRAS;


		public const int POTENTIALLY_INTERACTIBLE_MASK =
				ObjectLayer.UNITS_MASK |
				ObjectLayer.BUILDINGS_MASK |
				ObjectLayer.HEROES_MASK |
				ObjectLayer.EXTRAS_MASK;

		public const int SSOBJECTS_MASK =
				ObjectLayer.UNITS_MASK |
				ObjectLayer.BUILDINGS_MASK |
				ObjectLayer.PROJECTILES_MASK |
				ObjectLayer.HEROES_MASK |
				ObjectLayer.EXTRAS_MASK;
	

		public const int ALL_MASK =
			ObjectLayer.TERRAIN_MASK |

			ObjectLayer.UNITS_MASK |
			ObjectLayer.BUILDINGS_MASK |
			ObjectLayer.PROJECTILES_MASK |
			ObjectLayer.HEROES_MASK |
			ObjectLayer.EXTRAS_MASK;
	}
}