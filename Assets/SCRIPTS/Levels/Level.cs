using SS.Levels.SaveStates;

namespace SS.Levels
{
	/// <summary>
	/// Represents the level file. Contains data, that's unique to the level, and doesn't change throuought the gameplay.
	/// </summary>
	public struct Level
	{
		//
		//    General
		//

		/// <summary>
		/// The default save state is used when loading the level directly. This is the save state that's modified in the Level Editor.
		/// </summary>
		public LevelSaveState defaultSaveState { get; set; }

		/// <summary>
		/// Contains the file path directory name of the level.
		/// </summary>
		public string identifier { get; set; }

		/// <summary>
		/// Contains the name that, along with the save state's name, is displayed on the Save/Load menu.
		/// </summary>
		public string displayName { get; set; }

		//
		//    Factions
		//

		/// <summary>
		/// Contains the array of faction definitions for factions associated with this level.
		/// </summary>
		public FactionDefinition[] factions { get; set; }

		//
		//    Terrain
		//

		public float[,] heightMap { get; set; }
		public int MapSize { get; set; }
#warning incomplete - resolution, terrain textures, and change the method of mesh creation (possibly multithread).


		//
		//    Specialized Settings, used as extra parameters when creating level save states.
		//

		/// <summary>
		/// Contains extra information about what to save with save states. Not everything always needs to be saved (e.g. selection).
		/// </summary>
		LevelSaveSettings saveSettings { get; set; }


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		/// <summary>
		/// Loads the data from the level files into memory.
		/// </summary>
		/// <param name="directoryPath">The path to the level's root directory.</param>
		public void LoadDataFromFile( string directoryPath )
		{
			if( string.IsNullOrEmpty( directoryPath ) )
			{
				throw new System.ArgumentNullException( "Directory path san't be null or empty." );
			}
			throw new System.NotImplementedException();
#warning incomplete.
			// loads the level information from file (deserialization).
		}

		/// <summary>
		/// Saves the data from memory into the level file.
		/// </summary>
		/// <remarks>
		/// Used by the Level Editor when saving.
		/// </remarks>
		/// <param name="directoryPath">The path to the new level's root directory.</param>
		public void SaveDataToFile( string directoryPath )
		{
			if( string.IsNullOrEmpty( directoryPath ) )
			{
				throw new System.ArgumentNullException( "Directory path san't be null or empty." );
			}
			throw new System.NotImplementedException();
#warning incomplete.
		}

		/// <summary>
		/// Converts displayname into valid filename (removes special path characters).
		/// </summary>
		public static string IdentifierFromDisplayName( string displayName )
		{
#warning move this somewhere outside as it doesn't belong here.
#warning incomplete.
			const char REPLACEMENT_CHAR = '_';

			const char[] INVALID_CHARS = new char[] { '/', '\\', '?', '*', ':', '<', '>', '|' };

			throw new System.NotImplementedException();

			// replaces invalid path characters with REPLACEMENT_CHAR.
		}
	}
}