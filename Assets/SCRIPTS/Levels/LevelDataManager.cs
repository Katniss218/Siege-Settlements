using KFF;
using SS.Content;
using SS.Levels.SaveStates;

namespace SS.Levels
{
	/// <summary>
	/// Represents the level file. Contains data, that's unique to the level, and doesn't change throuought the gameplay.
	/// </summary>
	public static class LevelDataManager
	{
		//
		//    General
		//


		//
		//    Factions
		//

		public const int PLAYER_FAC = 0;

		static FactionDefinition[] _factions;
		public static FactionDefinition[] factions
		{
			get
			{
				return _factions;
			}
			set
			{
				// if factions are reset, reset faction data to null (expecting factino data to be set afterwards too).
				_factionData = null;
				_factions = value;
			}
		}
		
		static FactionData[] _factionData;
		public static FactionData[] factionData
		{
			get
			{
				return _factionData;
			}
			set
			{
				if( value != null && value.Length != _factions.Length )
				{
					throw new System.Exception( "Faction definitions array must have the same length as the faction data array." );
				}
				_factionData = value;
			}
		}
		
		//
		//    Quests
		//


		//
		//    Dialogues
		//
		
			


		//
		//    Specialized Settings, used as extra parameters when creating level save states.
		//

		/// <summary>
		/// Contains extra information about what to save with save states. Not everything always needs to be saved (e.g. selection).
		/// </summary>


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		

		public static void LoadFactions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "factions.kff" );

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

			
			int count = serializer.Analyze( "List" ).childCount;

			factions = new FactionDefinition[count];

			for( int i = 0; i < factions.Length; i++ )
			{
				factions[i] = new FactionDefinition();
			}

			serializer.DeserializeArray( "List", factions );
		}

		public static void LoadFactionData( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = LevelManager.GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_factions.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );


			int count = serializer.Analyze( "List" ).childCount;

			factionData = new FactionData[count];

			for( int i = 0; i < factionData.Length; i++ )
			{
				factionData[i] = new FactionData();
			}

			serializer.DeserializeArray( "List", factionData );
		}

		public static void SaveFactionData( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = LevelManager.GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_factions.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.SerializeArray( "", "List", factionData );
			
			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}
	}
}