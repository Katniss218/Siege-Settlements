using KFF;
using SS.Content;
using SS.Diplomacy;
using UnityEngine;

namespace SS.Levels
{
	/// <summary>
	/// Contains data, that's unique to the level, and doesn't change throuought the gameplay.
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

		public static int numFactions { get; private set; }

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

		public static RelationMap<DiplomaticRelation> diplomaticRelations { get; private set; }

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
			numFactions = count;

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
			if( count != numFactions )
			{
				throw new System.Exception( "The number of faction data doesn't match the number of factions of this level - '" + levelIdentifier + ":" + levelSaveStateIdentifier + "'." );
			}

			factionData = new FactionData[count];

			for( int i = 0; i < factionData.Length; i++ )
			{
				factionData[i] = new FactionData();
			}

			serializer.DeserializeArray( "List", factionData );


			int relMatCount = serializer.Analyze( "RelationMatrix" ).childCount;

			sbyte[] array = serializer.ReadSByteArray( "RelationMatrix" );

			diplomaticRelations = new RelationMap<DiplomaticRelation>( RelationMap<DiplomaticRelation>.GetSize( relMatCount ), DiplomaticRelation.Neutral );

			for( int i = 0; i < diplomaticRelations.MatrixLength; i++ )
			{
				diplomaticRelations[i] = (DiplomaticRelation)array[i];
			}
		}

		public static void LoadDaylightCycle( string levelIdentifier )
		{
			string path = LevelManager.GetLevelPath( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );
			
			int dayLength = serializer.ReadInt( "DaylightCycle.DayLength" );
			int nightLength = serializer.ReadInt( "DaylightCycle.NightLength" );

			float sunIntensity = serializer.ReadFloat( "DaylightCycle.SunIntensity" );
			float moonIntensity = serializer.ReadFloat( "DaylightCycle.MoonIntensity" );

			float sunElevationAngle = serializer.ReadFloat( "DaylightCycle.SunElevationAngle" );
			float moonElevationAngle = serializer.ReadFloat( "DaylightCycle.MoonElevationAngle" );


			DaylightCycleController daylightCycle = Object.FindObjectOfType<DaylightCycleController>();
			if( daylightCycle == null )
			{
				throw new System.Exception( "Couldn't find DaylightCycleController object." );
			}
			daylightCycle.dayLength = dayLength;
			daylightCycle.nightLength = nightLength;

			daylightCycle.sunIntensity = sunIntensity;
			daylightCycle.moonIntensity = moonIntensity;

			daylightCycle.sunElevationAngle = sunElevationAngle;
			daylightCycle.moonElevationAngle = moonElevationAngle;
		}

		public static void LoadDaylightCycleData( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = LevelManager.GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level_save_state.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

			float time = serializer.ReadFloat( "DaylightCycleData.Time" );


			DaylightCycleController daylightCycle = Object.FindObjectOfType<DaylightCycleController>();
			if( daylightCycle == null )
			{
				throw new System.Exception( "Couldn't find DaylightCycleController object." );
			}
			daylightCycle.time = time;
		}

		public static void LoadCameraData( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = LevelManager.GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level_save_state.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

			Vector3 pos = serializer.ReadVector3( "CameraData.Position" );
			Quaternion rot = serializer.ReadQuaternion( "CameraData.Rotation" );

			float orthSize = serializer.ReadFloat( "CameraData.ZoomSize" );


			CameraController camController = Object.FindObjectOfType<CameraController>();
			if( camController == null )
			{
				throw new System.Exception( "Couldn't find CameraController object." );
			}

			camController.transform.SetPositionAndRotation( pos, rot );
			camController.transform.GetChild( 0 ).GetComponent<Camera>().orthographicSize = orthSize;
		}

		//
		//
		//

		public static void SaveFactionData( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = LevelManager.GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_factions.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.SerializeArray( "", "List", factionData );

			
			sbyte[] array = new sbyte[diplomaticRelations.MatrixLength];
			
			for( int i = 0; i < array.Length; i++ )
			{
				array[i] = (sbyte)diplomaticRelations[i];
			}

			serializer.WriteSByteArray( "", "RelationMatrix", array );

			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}

		public static void SaveDaylightCycleData( KFFSerializer serializer )
		{
			DaylightCycleController daylightCycle = Object.FindObjectOfType<DaylightCycleController>();
			if( daylightCycle == null )
			{
				throw new System.Exception( "Couldn't find DaylightCycleController object." );
			}

			serializer.WriteClass( "", "DaylightCycleData" );
			serializer.WriteFloat( "DaylightCycleData", "Time", daylightCycle.time );
		}

		public static void SaveCameraData( KFFSerializer serializer )
		{
			CameraController camController = Object.FindObjectOfType<CameraController>();
			if( camController == null )
			{
				throw new System.Exception( "Couldn't find CameraController object." );
			}

			serializer.WriteClass( "", "CameraData" );
			serializer.WriteVector3( "CameraData", "Position", camController.transform.position );
			serializer.WriteQuaternion( "CameraData", "Rotation", camController.transform.rotation );

			serializer.WriteFloat( "CameraData", "ZoomSize", camController.transform.GetChild( 0 ).GetComponent<Camera>().orthographicSize );
		}
	}
}