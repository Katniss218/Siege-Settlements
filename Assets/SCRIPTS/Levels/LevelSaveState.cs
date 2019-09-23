namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Represents specific save state of a level. Contains data, that is unique to the save state, and can change throuought the gameplay.
	/// </summary>
	public struct LevelSaveState
	{
#warning save/load menu displays every save state of every level.
		//save states are contained in the specified level's directory and their names can be the same as save states of another level.
		// TODO - level1.savestateN.displayname == level2.savestateN.displayname

		public const string DEFAULT_SAVE_STATE_IDENTIFIER = "__default__"; // filename of default save state.
		public const string DEFAULT_SAVE_STATE_DISPLAYNAME = ""; // display name of default save state


		/// <summary>
		/// Contains the file path directory name of the save state.
		/// </summary>
		public string identifier { get; set; }

		/// <summary>
		/// Contains the name that, along with the level's name, is displayed on the Save/Load menu.
		/// </summary>
		public string displayName { get; set; }

		
		public UnitData[] unitSaveStates { get; set; }
		public BuildingData[] buildingSaveStates { get; set; }
		public ProjectileData[] projectileSaveStates { get; set; }
		public HeroData[] heroSaveStates { get; set; }
		public ExtraData[] extraSaveStates { get; set; }

#warning faction relations.
#warning faction technologies.


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		/// <summary>
		/// Returns every non-default save state of the specified level.
		/// </summary>
		/// <param name="levelDirectoryPath">The path to the level's root directory.</param>
		public static LevelSaveState[] GetAllCustomSaveStates( string levelDirectoryPath )
		{
			if( string.IsNullOrEmpty( levelDirectoryPath ) )
			{
				throw new System.ArgumentNullException( "Level directory path san't be null or empty." );
			}
			throw new System.NotImplementedException();
#warning incomplete.
		}
	}
}